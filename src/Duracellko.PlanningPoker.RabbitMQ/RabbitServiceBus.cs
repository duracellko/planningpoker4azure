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

    private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(5);

    private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
    private readonly IMessageConverter _messageConverter;
    private readonly GuidProvider _guidProvider;
    private readonly ILogger<RabbitServiceBus> _logger;

    private volatile string? _nodeId;
    private IConnection? _connection;
    private IModel? _receivingChannel;
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

        var shouldRetry = await Task.Run(() => SendMessage(message, false));
        if (shouldRetry)
        {
            await Task.Run(() => SendMessage(message, true));
        }
    }

    /// <summary>
    /// Register for receiving messages from other nodes.
    /// </summary>
    /// <param name="nodeId">Current node ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Register(string nodeId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nodeId);

        _nodeId = nodeId;
        _connection = CreateConnectionFactory().CreateConnection();
        _connection.CallbackException += ConnectionOnCallbackException;

        InitializeExchangeName();
        InitializeTopology();

        var receivingChannel = _receivingChannel!;
        var queueName = _queueName!;

        var consumer = new EventingBasicConsumer(receivingChannel);
        consumer.Received += ConsumerOnReceived;
        receivingChannel.BasicConsume(queueName, false, consumer);
        _logger.QueueCreated(_receivingExchangeName, nodeId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop receiving messages from other nodes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Unregister()
    {
        DeleteQueue();

        if (_connection != null)
        {
            _connection.Close();
            _connection.CallbackException -= ConnectionOnCallbackException;
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
        return Task.CompletedTask;
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
    private bool SendMessage(NodeMessage message, bool isRetry)
    {
        var connection = _connection;
        if (connection == null)
        {
            throw new InvalidOperationException(Resources.Error_RabbitMQNotInitialized);
        }

        IModel? sendingChannel = null;
        try
        {
            sendingChannel = connection.CreateModel();
            sendingChannel.ConfirmSelect();

            var properties = CreateBasicProperties(message, sendingChannel);
            properties.Headers = _messageConverter.GetMessageHeaders(message);
            var body = _messageConverter.GetMessageBody(message);

            sendingChannel.BasicPublish(_sendingExchangeName, string.Empty, properties, body);
            sendingChannel.WaitForConfirmsOrDie(PublishTimeout);
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
            sendingChannel?.Close();
            sendingChannel?.Dispose();
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Log error and try again.")]
    private void ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var nodeId = _nodeId;
        var receivingChannel = _receivingChannel;
        if (nodeId != null && receivingChannel != null)
        {
            var headers = e.BasicProperties.Headers;
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
                receivingChannel.BasicAck(e.DeliveryTag, false);
                _logger.MessageProcessed(_receivingExchangeName, nodeId, messageId);
            }
            catch (Exception ex)
            {
                receivingChannel.BasicNack(e.DeliveryTag, false, false);
                _logger.ErrorProcessMessage(ex, _receivingExchangeName, nodeId, messageId);
            }
        }
    }

    private void ConnectionOnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.ConnectionCallbackError(e.Exception, _receivingExchangeName, _nodeId);
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var uri = Configuration.ServiceBusConnectionString!;
        if (uri.StartsWith("RABBITMQ:", StringComparison.Ordinal))
        {
            uri = uri.Substring(9);
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
                _sendingExchangeName = topic.Substring(0, separatorIndex);
                _receivingExchangeName = topic.Substring(separatorIndex + 1);
            }
            else
            {
                _sendingExchangeName = topic;
                _receivingExchangeName = topic;
            }
        }
    }

    private void InitializeTopology()
    {
        var receivingChannel = _connection!.CreateModel();
        receivingChannel.ExchangeDeclare(_sendingExchangeName, ExchangeType.Fanout);
        receivingChannel.ExchangeDeclare(_receivingExchangeName, ExchangeType.Fanout);
        _receivingChannel = receivingChannel;

        var queue = receivingChannel.QueueDeclare(QueuePrefix + _nodeId, false, false, false);
        _queueName = queue.QueueName;
        receivingChannel.QueueBind(queue.QueueName, _receivingExchangeName, string.Empty);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue disposing other resources.")]
    private void DeleteQueue()
    {
        if (_queueName != null && _receivingChannel != null)
        {
            try
            {
                _receivingChannel.QueueDelete(_queueName);
                _queueName = null;
                _logger.QueueClosed(_receivingExchangeName, _nodeId);
            }
            catch (Exception ex)
            {
                _logger.ErrorClosingQueue(ex, _receivingExchangeName, _nodeId);
            }
        }
    }

    private IBasicProperties CreateBasicProperties(NodeMessage message, IModel model)
    {
        var properties = model.CreateBasicProperties();

        properties.MessageId = _guidProvider.NewGuid().ToString();
        properties.Type = message.MessageType.ToString();
        properties.Expiration = "60000"; // 1 minute

        if (message.Data != null)
        {
            properties.ContentType = message.Data is byte[] ? "application/octet-stream" : "application/json";
        }

        return properties;
    }
}
