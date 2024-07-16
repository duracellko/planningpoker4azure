using System;
using System.Collections.Generic;
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

    private readonly Subject<NodeMessage> _observableMessages = new Subject<NodeMessage>();
    private readonly IMessageConverter _messageConverter;
    private readonly GuidProvider _guidProvider;
    private readonly ILogger<RabbitServiceBus> _logger;

    private volatile string? _nodeId;
    private IConnection? _connection;
    private IModel? _receivingChannel;
    private string? _exchangeName;

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

        await Task.Run(() => SendMessageInternal(message));
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
        var queueName = InitializeTopology();

        var receivingChannel = _receivingChannel!;
        var consumer = new EventingBasicConsumer(receivingChannel);
        consumer.Received += ConsumerOnReceived;
        receivingChannel.BasicConsume(queueName, true, consumer);
        _logger.QueueCreated(_exchangeName, nodeId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop receiving messages from other nodes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Unregister()
    {
        if (_receivingChannel != null)
        {
            _receivingChannel.Close();
            _receivingChannel.Dispose();
            _receivingChannel = null;
        }

        if (_connection != null)
        {
            _connection.Close();
            _connection.CallbackException -= ConnectionOnCallbackException;
            _connection.Dispose();
            _connection = null;
        }

        if (!_observableMessages.IsDisposed)
        {
            _observableMessages.OnCompleted();
            _observableMessages.Dispose();
        }

        if (_nodeId != null)
        {
            _logger.QueueClosed(_exchangeName, _nodeId);
            _nodeId = null;
        }

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
    private void SendMessageInternal(NodeMessage message)
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

            var properties = CreateBasicProperties(message, sendingChannel);
            properties.Headers = _messageConverter.GetMessageHeaders(message);
            var body = _messageConverter.GetMessageBody(message);

            sendingChannel.BasicPublish(_exchangeName, string.Empty, properties, body);
            _logger.SendMessage(properties.MessageId);
        }
        catch (Exception ex)
        {
            _logger.ErrorSendMessage(ex);
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
        if (nodeId != null)
        {
            var headers = e.BasicProperties.Headers;
            var senderId = _messageConverter.GetHeader(headers, MessageConverter.SenderIdPropertyName);
            var recipientId = _messageConverter.GetHeader(headers, MessageConverter.RecipientIdPropertyName);

            var messageId = e.BasicProperties.MessageId;
            _logger.MessageReceived(_exchangeName, nodeId, messageId);

            if (senderId == nodeId || (recipientId != null && recipientId != nodeId))
            {
                return;
            }

            try
            {
                var nodeMessage = _messageConverter.GetNodeMessage(headers, e.Body);
                _observableMessages.OnNext(nodeMessage);
                _logger.MessageProcessed(_exchangeName, nodeId, messageId);
            }
            catch (Exception ex)
            {
                _logger.ErrorProcessMessage(ex, _exchangeName, nodeId, messageId);
            }
        }
    }

    private void ConnectionOnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.ConnectionCallbackError(e.Exception, _exchangeName, _nodeId);
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
        _exchangeName = Configuration.ServiceBusTopic;
        if (string.IsNullOrEmpty(_exchangeName))
        {
            _exchangeName = DefaultExchangeName;
        }
    }

    private string InitializeTopology()
    {
        var receivingChannel = _connection!.CreateModel();
        receivingChannel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout);

        var queueParameters = new Dictionary<string, object>()
        {
            { "message-ttl", 60000 }
        };
        var queue = receivingChannel.QueueDeclare(arguments: queueParameters);
        var queueName = queue.QueueName;
        receivingChannel.QueueBind(queueName, _exchangeName, string.Empty);

        _receivingChannel = receivingChannel;
        return queueName;
    }

    private IBasicProperties CreateBasicProperties(NodeMessage message, IModel model)
    {
        var properties = model.CreateBasicProperties();

        properties.MessageId = _guidProvider.NewGuid().ToString();
        properties.Type = message.MessageType.ToString();

        if (message.Data != null)
        {
            properties.ContentType = message.Data is byte[] ? "application/octet-stream" : "application/json";
        }

        return properties;
    }
}
