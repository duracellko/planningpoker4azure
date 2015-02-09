// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Duracellko.PlanningPoker.Azure.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// Sends and receives messages from Azure service bus.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class AzureServiceBus : IServiceBus, IDisposable
    {
        #region Fields

        private const string DefaultTopicName = "PlanningPoker";
        private const string ConnectionStringConfigurationName = "Microsoft.ServiceBus.ConnectionString";
        private const string TopicNameConfigurationName = "Microsoft.ServiceBus.TopicPath";
        private const string SubscriptionPingPropertyName = "SubscriptionPing";

        private readonly Subject<NodeMessage> observableMessages = new Subject<NodeMessage>();
        private readonly ConcurrentDictionary<string, DateTime> nodes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private volatile string nodeId;
        private string connectionString;
        private string topicName;
        private NamespaceManager namespaceManager;
        private Timer subscriptionsMaintenanceTimer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        /// <param name="messageConverter">The message converter.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        public AzureServiceBus(IMessageConverter messageConverter, IAzurePlanningPokerConfiguration configuration)
        {
            if (messageConverter == null)
            {
                throw new ArgumentNullException("messageConverter");
            }

            this.MessageConverter = messageConverter;
            this.Configuration = configuration ?? new AzurePlanningPokerConfigurationElement();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a message converter to convert messages from NodeMessage to BrokeredMessage and vice versa.
        /// </summary>
        public IMessageConverter MessageConverter { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        #endregion

        #region IServiceBus

        /// <summary>
        /// Gets an observable object receiving messages from service bus.
        /// </summary>
        public IObservable<NodeMessage> ObservableMessages
        {
            get
            {
                return this.observableMessages;
            }
        }

        /// <summary>
        /// Sends a message to service bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var brokeredMessage = this.MessageConverter.ConvertToBrokeredMessage(message);

            bool firstTry = true;
            Action<object> retrySendMessageAction = null;
            var sendMessageAction = new Action<object>(
                bm =>
                {
                    TopicClient topicClient = null;
                    try
                    {
                        topicClient = TopicClient.CreateFromConnectionString(this.connectionString, topicName);
                        topicClient.Send((BrokeredMessage)bm);
                    }
                    catch (Exception)
                    {
                        if (firstTry)
                        {
                            Task.Factory.StartNew(retrySendMessageAction, bm);
                        }
                        else
                        {
                            firstTry = false;
                        }
                    }
                    finally
                    {
                        if (topicClient != null)
                        {
                            topicClient.Close();
                        }
                    }
                });

            retrySendMessageAction = sendMessageAction;
            Task.Factory.StartNew(sendMessageAction, brokeredMessage);
        }

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        public void Register(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentNullException("nodeId");
            }

            this.nodeId = nodeId;
            this.connectionString = CloudConfigurationManager.GetSetting(ConnectionStringConfigurationName);
            this.topicName = CloudConfigurationManager.GetSetting(TopicNameConfigurationName);
            if (string.IsNullOrEmpty(this.topicName))
            {
                this.topicName = DefaultTopicName;
            }

            this.namespaceManager = NamespaceManager.CreateFromConnectionString(this.connectionString);

            this.CreateTopic();
            this.CreateSubscription();
            Task.Factory.StartNew(this.ReceiveMessages);

            this.SendSubscriptionIsAliveMessage();
            this.subscriptionsMaintenanceTimer = new Timer(this.Configuration.SubscriptionMaintenanceInterval.TotalMilliseconds);
            this.subscriptionsMaintenanceTimer.Elapsed += this.SubscriptionsMaintenanceTimerOnElapsed;
            this.subscriptionsMaintenanceTimer.Start();
        }

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        public void Unregister()
        {
            if (this.subscriptionsMaintenanceTimer != null)
            {
                this.subscriptionsMaintenanceTimer.Dispose();
                this.subscriptionsMaintenanceTimer = null;
            }

            this.observableMessages.OnCompleted();
            this.observableMessages.Dispose();

            this.DeleteSubscription();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
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
                this.Unregister();
            }
        }

        ~AzureServiceBus()
        {
            this.Dispose(false);
        }

        #endregion

        #region Private methods

        private void CreateTopic()
        {
            var topicDescription = new TopicDescription(this.topicName);
            topicDescription.DefaultMessageTimeToLive = TimeSpan.FromMinutes(1.5);

            if (!this.namespaceManager.TopicExists(this.topicName))
            {
                this.namespaceManager.CreateTopic(topicDescription);
            }
        }

        private void CreateSubscription()
        {
            if (!this.namespaceManager.SubscriptionExists(this.topicName, this.nodeId))
            {
                string sqlPattern = "{0} <> '{2}' AND ({1} IS NULL OR {1} = '{2}')";
                string senderIdPropertyName = ServiceBus.MessageConverter.SenderIdPropertyName;
                string recipientIdPropertyName = ServiceBus.MessageConverter.RecipientIdPropertyName;
                var filter = new SqlFilter(string.Format(CultureInfo.InvariantCulture, sqlPattern, senderIdPropertyName, recipientIdPropertyName, this.nodeId));
                this.namespaceManager.CreateSubscription(this.topicName, this.nodeId, filter);
            }
        }

        private void DeleteSubscription()
        {
            if (this.nodeId != null)
            {
                this.namespaceManager.DeleteSubscription(this.topicName, this.nodeId);
                this.nodeId = null;
            }
        }

        private void ReceiveMessages()
        {
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(this.connectionString, this.topicName, this.nodeId);
            try
            {
                while (this.nodeId != null)
                {
                    try
                    {
                        var brokeredMessage = subscriptionClient.Receive();
                        if (brokeredMessage != null)
                        {
                            try
                            {
                                if (brokeredMessage.Properties.ContainsKey(SubscriptionPingPropertyName))
                                {
                                    this.ProcessSubscriptionIsAliveMessage(brokeredMessage);
                                }
                                else
                                {
                                    var message = this.MessageConverter.ConvertToNodeMessage(brokeredMessage);
                                    this.observableMessages.OnNext(message);
                                }

                                brokeredMessage.Complete();
                            }
                            catch (Exception)
                            {
                                brokeredMessage.Abandon();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // just try receive message again
                    }
                }
            }
            finally
            {
                subscriptionClient.Close();
            }
        }

        #region Subscriptions maintenance

        private void SubscriptionsMaintenanceTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            this.SendSubscriptionIsAliveMessage();
            this.DeleteInactiveSubscriptions();
        }

        private void DeleteInactiveSubscriptions()
        {
            var subscriptions = this.namespaceManager.GetSubscriptions(this.topicName);
            foreach (var subscription in subscriptions)
            {
                if (!string.Equals(subscription.Name, this.nodeId, StringComparison.OrdinalIgnoreCase))
                {
                    // if subscription is new, then assume that it has been created very recently and
                    // this node has not received notification about it yet
                    this.nodes.TryAdd(subscription.Name, DateTime.UtcNow);

                    DateTime lastSubscriptionActivity;
                    if (this.nodes.TryGetValue(subscription.Name, out lastSubscriptionActivity))
                    {
                        if (lastSubscriptionActivity < DateTime.UtcNow - this.Configuration.SubscriptionInactivityTimeout)
                        {
                            this.namespaceManager.DeleteSubscription(subscription.TopicPath, subscription.Name);
                        }
                    }
                }
            }
        }

        private void ProcessSubscriptionIsAliveMessage(BrokeredMessage message)
        {
            var subscriptionLastActivityTime = (DateTime)message.Properties[SubscriptionPingPropertyName];
            var subscriptionId = (string)message.Properties[ServiceBus.MessageConverter.SenderIdPropertyName];
            this.nodes[subscriptionId] = subscriptionLastActivityTime;
        }

        private void SendSubscriptionIsAliveMessage()
        {
            var message = new BrokeredMessage();
            message.Properties[SubscriptionPingPropertyName] = DateTime.UtcNow;
            message.Properties[ServiceBus.MessageConverter.SenderIdPropertyName] = this.nodeId;
            TopicClient topicClient = null;
            try
            {
                topicClient = TopicClient.CreateFromConnectionString(this.connectionString, this.topicName);
                topicClient.Send(message);
            }
            finally
            {
                if (topicClient != null)
                {
                    topicClient.Close();
                }
            }
        }

        #endregion

        #endregion
    }
}
