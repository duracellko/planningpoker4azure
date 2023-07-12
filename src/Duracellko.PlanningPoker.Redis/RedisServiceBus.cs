using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Duracellko.PlanningPoker.Redis
{
    /// <summary>
    /// Sends and receives messages from Redis service bus.
    /// </summary>
    public class RedisServiceBus : IServiceBus, IDisposable
    {
        private const string DefaultChannelName = "PlanningPoker";

        private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
        private readonly ILogger<RedisServiceBus> _logger;

        private volatile string? _nodeId;
        private string? _channel;
        private RedisChannel _redisChannel;
        private ConnectionMultiplexer? _redis;
        private ISubscriber? _subscriber;
        private bool _subscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisServiceBus"/> class.
        /// </summary>
        /// <param name="messageConverter">The message converter.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public RedisServiceBus(IRedisMessageConverter messageConverter, IAzurePlanningPokerConfiguration configuration, ILogger<RedisServiceBus> logger)
        {
            MessageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RedisServiceBus"/> class.
        /// </summary>
        ~RedisServiceBus()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a message converter to convert messages from NodeMessage to RedisValue and vice versa.
        /// </summary>
        public IRedisMessageConverter MessageConverter { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets an observable object receiving messages from service bus.
        /// </summary>
        public IObservable<NodeMessage> ObservableMessages => _observableMessages;

        /// <summary>
        /// Sends a message to Redis channel.
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

            var subscriber = _subscriber;
            if (subscriber == null)
            {
                throw new InvalidOperationException(Resources.Error_RedisPubSubNotInitialized);
            }

            var redisMessage = MessageConverter.ConvertToRedisMessage(message);

            try
            {
                await subscriber.PublishAsync(_redisChannel, redisMessage, CommandFlags.FireAndForget);
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
            _channel = Configuration.ServiceBusTopic;
            if (string.IsNullOrEmpty(_channel))
            {
                _channel = DefaultChannelName;
            }

            var connectionString = GetConnectionString();
            _redisChannel = new RedisChannel(_channel, RedisChannel.PatternMode.Literal);
            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _subscriber = _redis.GetSubscriber();
            await CreateSubscription(_redisChannel, _channel, nodeId);
        }

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Unregister()
        {
            if (_subscribed)
            {
                if (_subscriber != null)
                {
                    await _subscriber.UnsubscribeAsync(_redisChannel, null, CommandFlags.FireAndForget);
                    _subscriber = null;
                }

                _subscribed = false;
            }

            if (!_observableMessages.IsDisposed)
            {
                _observableMessages.OnCompleted();
                _observableMessages.Dispose();
            }

            if (_redis != null)
            {
                await _redis.CloseAsync();
                await _redis.DisposeAsync();
                _redis = null;
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

        private async Task CreateSubscription(RedisChannel redisChannel, string channelName, string nodeId)
        {
            if (!_subscribed)
            {
                await _subscriber!.SubscribeAsync(redisChannel, ReceiveMessage);
                _subscribed = true;
                _logger.SubscriptionCreated(channelName, nodeId);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error and try again.")]
        private void ReceiveMessage(RedisChannel redisChannel, RedisValue redisValue)
        {
            var nodeId = _nodeId;
            if (redisValue.HasValue && nodeId != null)
            {
                var nodeMessage = MessageConverter.GetMessageHeader(redisValue);
                var messageId = $"{nodeMessage.SenderNodeId}${nodeMessage.RecipientNodeId}${nodeMessage.MessageType}";
                _logger.MessageReceived(_channel, nodeId, messageId);

                if (nodeMessage.SenderNodeId == nodeId ||
                    (nodeMessage.RecipientNodeId != null && nodeMessage.RecipientNodeId != nodeId))
                {
                    return;
                }

                try
                {
                    nodeMessage = MessageConverter.ConvertToNodeMessage(redisValue);
                    _observableMessages.OnNext(nodeMessage);
                    _logger.MessageProcessed(_channel, nodeId, messageId);
                }
                catch (Exception ex)
                {
                    _logger.ErrorProcessMessage(ex, _channel, nodeId, messageId);
                }
            }
        }

        private string GetConnectionString()
        {
            var connectionString = Configuration.ServiceBusConnectionString!;
            if (connectionString.StartsWith("REDIS:", StringComparison.Ordinal))
            {
                connectionString = connectionString.Substring(6);
            }

            return connectionString;
        }
    }
}
