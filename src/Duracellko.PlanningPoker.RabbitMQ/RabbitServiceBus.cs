using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Duracellko.PlanningPoker.RabbitMQ;

/// <summary>
/// Sends and receives messages via service bus using RabbitMQ.
/// </summary>
public class RabbitServiceBus : IServiceBus, IDisposable
{
    private const string DefaultExchangeName = "PlanningPoker";
    private const string QueuePrefix = "PlanningPoker-";

    private readonly Subject<NodeMessage> _observableMessages = new();
    private readonly IMessageConverter _messageConverter;
    private readonly GuidProvider _guidProvider;
    private readonly ILogger<RabbitServiceBus> _logger;

    private volatile string? _nodeId;
    private IConnection? _connection;
    private IChannel? _receivingChannel;
    private string? _sendingExchangeName;
    private string? _receivingExchangeName;
    private string? _queueName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitServiceBus"/> class.
    /// </summary>
    /// <param name="messageConverter">The message converter.</param>
    /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
    /// <param name="guidProvider">The GUID provider to provide new GUID objects.</param>
    /// <param name="logger">Logger instance to log events.</param>
    public RabbitServiceBus(
        IMessageConverter messageConverter,
        IAzurePlanningPokerConfiguration configuration,
        GuidProvider? guidProvider,
        ILogger<RabbitServiceBus> logger)
    {
        _messageConverter = messageConverter ?? throw new ArgumentNullException(nameof(messageConverter));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _guidProvider = guidProvider ?? GuidProvider.Default;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a value indicating whether the connection to RabbitMQ is opened.
    /// </summary>
    public bool IsConnected => _connection != null && _connection.IsOpen;

    /// <summary>
    /// Gets a configuration of planning poker for Azure platform.
    /// </summary>
    public IAzurePlanningPokerConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets an observable object receiving messages from service bus.
    /// </summary>
    public IObservable<NodeMessage> ObservableMessages => _observableMessages;

    /// <summary>
    /// Sends a message to RabbitMQ exchange.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendMessage(NodeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var shouldRetry = await SendMessage(message, false);
        if (shouldRetry)
        {
            await SendMessage(message, true);
        }
    }

    /// <summary>
    /// Register for receiving messages from other nodes.
    /// </summary>
    /// <param name="nodeId">Current node ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Register(string nodeId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nodeId);

        _nodeId = nodeId;
        _connection = await CreateConnectionFactory().CreateConnectionAsync();
        _connection.CallbackExceptionAsync += ConnectionOnCallbackException;

        InitializeExchangeName();
        await InitializeTopology();

        var receivingChannel = _receivingChannel!;
        var queueName = _queueName!;

        var consumer = new AsyncEventingBasicConsumer(receivingChannel);
        consumer.ReceivedAsync += ConsumerOnReceived;
        await receivingChannel.BasicConsumeAsync(queueName, false, consumer);
        _logger.QueueCreated(_receivingExchangeName, nodeId);
    }

    /// <summary>
    /// Stop receiving messages from other nodes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Unregister()
    {
        await DeleteQueue();

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.CallbackExceptionAsync -= ConnectionOnCallbackException;
            _connection.Dispose();
            _connection = null;
        }

        if (_receivingChannel != null)
        {
            _receivingChannel.Dispose();
            _receivingChannel = null;
        }

        if (!_observableMessages.IsDisposed)
        {
            _observableMessages.OnCompleted();
            _observableMessages.Dispose();
        }

        _nodeId = null;
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

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error.")]
    private async ValueTask<bool> SendMessage(NodeMessage message, bool isRetry)
    {
        var connection = _connection;
        if (connection == null)
        {
            throw new InvalidOperationException(Resources.Error_RabbitMQNotInitialized);
        }

        IChannel? sendingChannel = null;
        try
        {
            var channelOptions = new CreateChannelOptions(true, false);
            sendingChannel = await connection.CreateChannelAsync(channelOptions);

            var properties = CreateBasicProperties(message);
            properties.Headers = _messageConverter.GetMessageHeaders(message);
            var body = _messageConverter.GetMessageBody(message);

            System.Diagnostics.Debug.Assert(_sendingExchangeName != null, "SendingExchangeName should not be null, when connection is opened.");
            await sendingChannel.BasicPublishAsync(_sendingExchangeName, string.Empty, false, properties, body);
            _logger.SendMessage(properties.MessageId);
            return false;
        }
        catch (Exception ex)
        {
            if (isRetry)
            {
                _logger.ErrorSendMessage(ex);
            }

            return !isRetry;
        }
        finally
        {
            if (sendingChannel != null)
            {
                await sendingChannel.CloseAsync();
            }

            sendingChannel?.Dispose();
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error and try again.")]
    private async Task ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var nodeId = _nodeId;
        var receivingChannel = _receivingChannel;
        if (nodeId != null && receivingChannel != null)
        {
            var headers = e.BasicProperties.Headers;
            System.Diagnostics.Debug.Assert(headers != null, "Receiving message headers should not be null.");
            var senderId = _messageConverter.GetHeader(headers, MessageConverter.SenderIdPropertyName);
            var recipientId = _messageConverter.GetHeader(headers, MessageConverter.RecipientIdPropertyName);

            var messageId = e.BasicProperties.MessageId;
            _logger.MessageReceived(_receivingExchangeName, nodeId, messageId);

            if (senderId == nodeId || (recipientId != null && recipientId != nodeId))
            {
                return;
            }

            try
            {
                var nodeMessage = _messageConverter.GetNodeMessage(headers, e.Body);
                _observableMessages.OnNext(nodeMessage);
                await receivingChannel.BasicAckAsync(e.DeliveryTag, false);
                _logger.MessageProcessed(_receivingExchangeName, nodeId, messageId);
            }
            catch (Exception ex)
            {
                await receivingChannel.BasicNackAsync(e.DeliveryTag, false, false);
                _logger.ErrorProcessMessage(ex, _receivingExchangeName, nodeId, messageId);
            }
        }
    }

    private Task ConnectionOnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.ConnectionCallbackError(e.Exception, _receivingExchangeName, _nodeId);
        return Task.CompletedTask;
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var uri = Configuration.ServiceBusConnectionString!;
        if (uri.StartsWith("RABBITMQ:", StringComparison.Ordinal))
        {
            uri = uri[9..];
        }

        return new ConnectionFactory()
        {
            Uri = new Uri(uri)
        };
    }

    private void InitializeExchangeName()
    {
        var topic = Configuration.ServiceBusTopic;
        if (string.IsNullOrEmpty(topic))
        {
            _sendingExchangeName = DefaultExchangeName;
            _receivingExchangeName = DefaultExchangeName;
        }
        else
        {
            var separatorIndex = topic.IndexOf(';', StringComparison.Ordinal);
            if (separatorIndex > 0 && separatorIndex < topic.Length - 1)
            {
                _sendingExchangeName = topic[..separatorIndex];
                _receivingExchangeName = topic[(separatorIndex + 1)..];
            }
            else
            {
                _sendingExchangeName = topic;
                _receivingExchangeName = topic;
            }
        }
    }

    private async ValueTask InitializeTopology()
    {
        var receivingChannel = await _connection!.CreateChannelAsync();

        System.Diagnostics.Debug.Assert(_sendingExchangeName != null, "SendingExchangeName should not be null, when connection is opened.");
        await receivingChannel.ExchangeDeclareAsync(_sendingExchangeName, ExchangeType.Fanout);
        System.Diagnostics.Debug.Assert(_receivingExchangeName != null, "ReceivingExchangeName should not be null, when connection is opened.");
        await receivingChannel.ExchangeDeclareAsync(_receivingExchangeName, ExchangeType.Fanout);
        _receivingChannel = receivingChannel;

        var queue = await receivingChannel.QueueDeclareAsync(QueuePrefix + _nodeId, false, false, false);
        _queueName = queue.QueueName;
        await receivingChannel.QueueBindAsync(queue.QueueName, _receivingExchangeName, string.Empty);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue disposing other resources.")]
    private async ValueTask DeleteQueue()
    {
        if (_queueName != null && _receivingChannel != null)
        {
            try
            {
                await _receivingChannel.QueueDeleteAsync(_queueName);
                _queueName = null;
                _logger.QueueClosed(_receivingExchangeName, _nodeId);
            }
            catch (Exception ex)
            {
                _logger.ErrorClosingQueue(ex, _receivingExchangeName, _nodeId);
            }
        }
    }

    private BasicProperties CreateBasicProperties(NodeMessage message)
    {
        var properties = new BasicProperties
        {
            MessageId = _guidProvider.NewGuid().ToString(),
            Type = message.MessageType.ToString(),
            Expiration = "60000" // 1 minute
        };

        if (message.Data != null)
        {
            properties.ContentType = (message.Data is byte[]) ? "application/octet-stream" : "application/json";
        }

        return properties;
    }
}
