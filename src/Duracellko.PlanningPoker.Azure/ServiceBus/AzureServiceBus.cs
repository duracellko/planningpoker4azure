using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Timers;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Duracellko.PlanningPoker.Azure.Configuration;
using Microsoft.Extensions.Logging;

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

        private static readonly TimeSpan _serviceBusTokenTimeOut = TimeSpan.FromMinutes(1);

        private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
        private readonly ConcurrentDictionary<string, DateTime> _nodes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<AzureServiceBus> _logger;

        private volatile string _nodeId;
        private string _connectionString;
        private string _topicName;
        private ServiceBusAdministrationClient _serviceBusAdministrationClient;
        private ServiceBusClient _serviceBusClient;
        private ServiceBusSender _serviceBusSender;
        private ServiceBusProcessor _serviceBusProcessor;
        private System.Timers.Timer _subscriptionsMaintenanceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        /// <param name="messageConverter">The message converter.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public AzureServiceBus(IMessageConverter messageConverter, IAzurePlanningPokerConfiguration configuration, ILogger<AzureServiceBus> logger)
        {
            MessageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error.")]
        public async Task SendMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var serviceBusSender = _serviceBusSender;
            if (serviceBusSender == null)
            {
                throw new InvalidOperationException("AzureServiceBus is not initialized.");
            }

            var serviceBusMessage = MessageConverter.ConvertToServiceBusMessage(message);

            try
            {
                await serviceBusSender.SendMessageAsync(serviceBusMessage);
                _logger.SendMessage();
            }
            catch (Exception ex)
            {
                _logger.ErrorSendMessage(ex);
            }
        }

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Register(string nodeId)
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

            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(_connectionString);
            _serviceBusClient = new ServiceBusClient(_connectionString);
            _serviceBusSender = _serviceBusClient.CreateSender(_topicName);

            await CreateSubscription();

            await SendSubscriptionIsAliveMessage();
            _subscriptionsMaintenanceTimer = new System.Timers.Timer(Configuration.SubscriptionMaintenanceInterval.TotalMilliseconds);
            _subscriptionsMaintenanceTimer.Elapsed += SubscriptionsMaintenanceTimerOnElapsed;
            _subscriptionsMaintenanceTimer.Start();
        }

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Unregister()
        {
            if (_subscriptionsMaintenanceTimer != null)
            {
                _subscriptionsMaintenanceTimer.Dispose();
                _subscriptionsMaintenanceTimer = null;
            }

            if (_serviceBusProcessor != null)
            {
                await _serviceBusProcessor.DisposeAsync();
                _serviceBusProcessor = null;
            }

            if (!_observableMessages.IsDisposed)
            {
                _observableMessages.OnCompleted();
                _observableMessages.Dispose();
            }

            if (_serviceBusSender != null)
            {
                await _serviceBusSender.DisposeAsync();
                _serviceBusSender = null;
            }

            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync();
                _serviceBusClient = null;
            }

            if (_serviceBusAdministrationClient != null)
            {
                await DeleteSubscription();
                _serviceBusAdministrationClient = null;
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
                Unregister().Wait();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        ~AzureServiceBus()
        {
            Dispose(false);
        }

        private async Task CreateSubscription()
        {
            if (_serviceBusProcessor == null)
            {
                await CreateTopicSubscription(_topicName, _nodeId);

                var processorOptions = new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false
                };
                var serviceBusProcessor = _serviceBusClient.CreateProcessor(_topicName, _nodeId, processorOptions);
                serviceBusProcessor.ProcessMessageAsync += ReceiveMessage;
                serviceBusProcessor.ProcessErrorAsync += ProcessError;
                _serviceBusProcessor = serviceBusProcessor;
                await serviceBusProcessor.StartProcessingAsync();
                _logger.SubscriptionCreated(_topicName, _nodeId);
            }
        }

        private Task CreateTopicSubscription(string topicName, string nodeId)
        {
            var subscriptionOptions = new CreateSubscriptionOptions(topicName, nodeId)
            {
                DefaultMessageTimeToLive = _serviceBusTokenTimeOut
            };

            string sqlPattern = "{0} <> '{2}' AND ({1} IS NULL OR {1} = '{2}')";
            string senderIdPropertyName = ServiceBus.MessageConverter.SenderIdPropertyName;
            string recipientIdPropertyName = ServiceBus.MessageConverter.RecipientIdPropertyName;
            var filter = new SqlRuleFilter(string.Format(CultureInfo.InvariantCulture, sqlPattern, senderIdPropertyName, recipientIdPropertyName, nodeId));
            var subscriptionRuleOptions = new CreateRuleOptions("RecipientFilter", filter);

            return _serviceBusAdministrationClient.CreateSubscriptionAsync(subscriptionOptions, subscriptionRuleOptions);
        }

        private async Task DeleteSubscription()
        {
            if (_nodeId != null)
            {
                await DeleteSubscription(_nodeId);
                _logger.SubscriptionDeleted(_topicName, _nodeId);
                _nodeId = null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error and try again.")]
        private async Task ReceiveMessage(ProcessMessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var cancellationToken = messageEventArgs.CancellationToken;

            if (message != null && _nodeId != null)
            {
                _logger.MessageReceived(_topicName, _nodeId, message.MessageId);

                try
                {
                    if (message.ApplicationProperties.ContainsKey(SubscriptionPingPropertyName))
                    {
                        ProcessSubscriptionIsAliveMessage(message);
                    }
                    else
                    {
                        var nodeMessage = MessageConverter.ConvertToNodeMessage(message);
                        _observableMessages.OnNext(nodeMessage);
                    }

                    await messageEventArgs.CompleteMessageAsync(message, cancellationToken);
                    _logger.MessageProcessed(_topicName, _nodeId, message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.ErrorProcessMessage(ex, _topicName, _nodeId, message.MessageId);
                    await messageEventArgs.AbandonMessageAsync(message, null, cancellationToken);
                }
            }
        }

        private Task ProcessError(ProcessErrorEventArgs errorEventArgs)
        {
            _logger.ErrorProcess(errorEventArgs.Exception, _topicName, _nodeId, errorEventArgs.ErrorSource);
            return Task.CompletedTask;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error.")]
        private async void SubscriptionsMaintenanceTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendSubscriptionIsAliveMessage();
                await DeleteInactiveSubscriptions();
            }
            catch (Exception ex)
            {
                _logger.ErrorSubscriptionsMaintenance(ex, _nodeId);
            }
        }

        private void ProcessSubscriptionIsAliveMessage(ServiceBusReceivedMessage message)
        {
            var subscriptionLastActivityTime = (DateTime)message.ApplicationProperties[SubscriptionPingPropertyName];
            var subscriptionId = (string)message.ApplicationProperties[ServiceBus.MessageConverter.SenderIdPropertyName];
            _logger.SubscriptionAliveMessageReceived(_topicName, _nodeId, subscriptionId);
            _nodes[subscriptionId] = subscriptionLastActivityTime;
        }

        [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "topicClient can be null, when constructor fails.")]
        private async Task SendSubscriptionIsAliveMessage()
        {
            var message = new ServiceBusMessage();
            message.ApplicationProperties[SubscriptionPingPropertyName] = DateTime.UtcNow;
            message.ApplicationProperties[ServiceBus.MessageConverter.SenderIdPropertyName] = _nodeId;
            await _serviceBusSender.SendMessageAsync(message);
            _logger.SubscriptionAliveSent(_nodeId);
        }

        private async Task DeleteInactiveSubscriptions()
        {
            var subscriptions = await GetTopicSubscriptions();
            foreach (var subscription in subscriptions)
            {
                if (!string.Equals(subscription, _nodeId, StringComparison.OrdinalIgnoreCase))
                {
                    // if subscription is new, then assume that it has been created very recently and
                    // this node has not received notification about it yet
                    _nodes.TryAdd(subscription, DateTime.UtcNow);

                    DateTime lastSubscriptionActivity;
                    if (_nodes.TryGetValue(subscription, out lastSubscriptionActivity))
                    {
                        if (lastSubscriptionActivity < DateTime.UtcNow - Configuration.SubscriptionInactivityTimeout)
                        {
                            await DeleteSubscription(subscription);
                            _nodes.TryRemove(subscription, out lastSubscriptionActivity);
                            _logger.InactiveSubscriptionDeleted(_nodeId, subscription);
                        }
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> GetTopicSubscriptions()
        {
            var subscriptions = await _serviceBusAdministrationClient.GetSubscriptionsAsync(_topicName).ToListAsync();
            return subscriptions.Select(s => s.SubscriptionName);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Subscription will be deleted next time.")]
        private async Task DeleteSubscription(string name)
        {
            try
            {
                var existsSubcription = await _serviceBusAdministrationClient.SubscriptionExistsAsync(_topicName, name);
                if (existsSubcription)
                {
                    await _serviceBusAdministrationClient.DeleteSubscriptionAsync(_topicName, name);
                }
            }
            catch (Exception ex)
            {
                _logger.SubscriptionDeleteFailed(ex, _topicName, name);
            }
        }
    }
}
