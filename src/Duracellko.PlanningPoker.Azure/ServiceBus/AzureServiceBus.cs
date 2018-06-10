using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using Duracellko.PlanningPoker.Azure.Configuration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// Sends and receives messages from Azure service bus.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class AzureServiceBus : IServiceBus, IDisposable
    {
        private const string DefaultTopicName = "PlanningPoker";
        private const string SubscriptionPingPropertyName = "SubscriptionPing";

        private static readonly XNamespace _entityNamespace = XNamespace.Get("http://www.w3.org/2005/Atom");
        private static readonly XNamespace _servicebusNamespace = XNamespace.Get("http://schemas.microsoft.com/netservices/2010/10/servicebus/connect");
        private static readonly TimeSpan _serviceBusTokenTimeOut = TimeSpan.FromMinutes(1);

        private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
        private readonly ConcurrentDictionary<string, DateTime> _nodes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private volatile string _nodeId;
        private string _connectionString;
        private string _topicName;
        private SubscriptionClient _subscriptionClient;
        private System.Timers.Timer _subscriptionsMaintenanceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        /// <param name="messageConverter">The message converter.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        public AzureServiceBus(IMessageConverter messageConverter, IAzurePlanningPokerConfiguration configuration)
        {
            MessageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets a message converter to convert messages from NodeMessage to BrokeredMessage and vice versa.
        /// </summary>
        public IMessageConverter MessageConverter { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets an observable object receiving messages from service bus.
        /// </summary>
        public IObservable<NodeMessage> ObservableMessages
        {
            get
            {
                return _observableMessages;
            }
        }

        /// <summary>
        /// Sends a message to service bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public async void SendMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var topicMessage = MessageConverter.ConvertToBrokeredMessage(message);

            TopicClient topicClient = null;
            try
            {
                topicClient = new TopicClient(_connectionString, _topicName);
                await topicClient.SendAsync(topicMessage);
            }
            catch (Exception)
            {
                // ignore
            }
            finally
            {
                if (topicClient != null)
                {
                    await topicClient.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        public async void Register(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            _nodeId = nodeId;
            _connectionString = Configuration.ServiceBusConnectionString;
            _topicName = Configuration.ServiceBusTopic;
            if (string.IsNullOrEmpty(_topicName))
            {
                _topicName = DefaultTopicName;
            }

            await CreateSubscription();

            SendSubscriptionIsAliveMessage();
            _subscriptionsMaintenanceTimer = new System.Timers.Timer(Configuration.SubscriptionMaintenanceInterval.TotalMilliseconds);
            _subscriptionsMaintenanceTimer.Elapsed += SubscriptionsMaintenanceTimerOnElapsed;
            _subscriptionsMaintenanceTimer.Start();
        }

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        public void Unregister()
        {
            if (_subscriptionsMaintenanceTimer != null)
            {
                _subscriptionsMaintenanceTimer.Dispose();
                _subscriptionsMaintenanceTimer = null;
            }

            if (_subscriptionClient != null)
            {
                _subscriptionClient.CloseAsync().Wait();
                _subscriptionClient = null;
            }

            if (!_observableMessages.IsDisposed)
            {
                _observableMessages.OnCompleted();
                _observableMessages.Dispose();
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> if disposing not using GC; otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unregister();
            }
        }

        ~AzureServiceBus()
        {
            Dispose(false);
        }

        private static XDocument CreateSubscriptionDescription()
        {
            var subscriptionDescription = new XElement(
                _servicebusNamespace + "SubscriptionDescription",
                new XElement(_servicebusNamespace + "DefaultMessageTimeToLive", TimeSpan.FromMinutes(1)));
            return new XDocument(
                new XElement(
                    _entityNamespace + "entry",
                    new XElement(_entityNamespace + "content", new XAttribute("type", "application/xml"), subscriptionDescription)));
        }

        private async Task CreateSubscription()
        {
            if (_subscriptionClient == null)
            {
                using (var httpClient = new HttpClient())
                {
                    var uri = GetSubscriptionUri(_nodeId);
                    var tokenProvider = CreateTokenProvider();
                    var token = await tokenProvider.GetTokenAsync(uri.ToString(), _serviceBusTokenTimeOut);
                    httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token.TokenValue);

                    using (var content = new StringContent(CreateSubscriptionDescription().ToString(SaveOptions.None), Encoding.UTF8, "application/atom+xml"))
                    {
                        content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "entry"));
                        var subscriptionResponse = await httpClient.PutAsync(uri, content);
                        subscriptionResponse.Dispose();
                    }
                }

                _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, _nodeId);

                string sqlPattern = "{0} <> '{2}' AND ({1} IS NULL OR {1} = '{2}')";
                string senderIdPropertyName = ServiceBus.MessageConverter.SenderIdPropertyName;
                string recipientIdPropertyName = ServiceBus.MessageConverter.RecipientIdPropertyName;
                var filter = new SqlFilter(string.Format(CultureInfo.InvariantCulture, sqlPattern, senderIdPropertyName, recipientIdPropertyName, _nodeId));
                await _subscriptionClient.AddRuleAsync("RecipientFilter", filter);

                _subscriptionClient.RegisterMessageHandler(ReceiveMessage, ex => Task.CompletedTask);
            }
        }

        private async Task ReceiveMessage(Message message, CancellationToken cancellationToken)
        {
            if (message != null)
            {
                try
                {
                    if (message.UserProperties.ContainsKey(SubscriptionPingPropertyName))
                    {
                        ProcessSubscriptionIsAliveMessage(message);
                    }
                    else
                    {
                        var nodeMessage = MessageConverter.ConvertToNodeMessage(message);
                        _observableMessages.OnNext(nodeMessage);
                    }

                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                }
                catch (Exception)
                {
                    await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
        }

        private void SubscriptionsMaintenanceTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            SendSubscriptionIsAliveMessage();
        }

        private void ProcessSubscriptionIsAliveMessage(Message message)
        {
            var subscriptionLastActivityTime = (DateTime)message.UserProperties[SubscriptionPingPropertyName];
            var subscriptionId = (string)message.UserProperties[ServiceBus.MessageConverter.SenderIdPropertyName];
            _nodes[subscriptionId] = subscriptionLastActivityTime;
        }

        private async void SendSubscriptionIsAliveMessage()
        {
            var message = new Message();
            message.UserProperties[SubscriptionPingPropertyName] = DateTime.UtcNow;
            message.UserProperties[ServiceBus.MessageConverter.SenderIdPropertyName] = _nodeId;
            TopicClient topicClient = null;
            try
            {
                topicClient = new TopicClient(_connectionString, _topicName);
                await topicClient.SendAsync(message);
            }
            finally
            {
                if (topicClient != null)
                {
                    await topicClient.CloseAsync();
                }
            }
        }

        private ITokenProvider CreateTokenProvider()
        {
            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(_connectionString);
            return TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SasKeyName, connectionStringBuilder.SasKey, TokenScope.Namespace);
        }

        private Uri GetSubscriptionUri(string subcriptionName)
        {
            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(_connectionString);
            var uri = connectionStringBuilder.Endpoint.Replace("sb://", "https://");
            uri = $"{uri}/{Uri.EscapeDataString(_topicName)}/subscriptions/{Uri.EscapeDataString(subcriptionName)}";
            return new Uri(uri);
        }
    }
}
