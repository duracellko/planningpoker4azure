using System;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.RabbitMQ;

internal static class RabbitServiceBusLogger
{
    private const int BaseEventId = 1750;

    private static readonly Action<ILogger, string?, Exception?> _sendMessage = LoggerMessage.Define<string?>(
        LogLevel.Debug,
        new EventId(BaseEventId + 1, nameof(SendMessage)),
        "Message sent to RabbitMQ. MessageID: {MessageId}");

    private static readonly Action<ILogger, Exception?> _errorSendMessage = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(BaseEventId + 2, nameof(ErrorSendMessage)),
        "Error sending message to RabbitMQ.");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageReceived = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Debug,
        new EventId(BaseEventId + 3, nameof(MessageReceived)),
        "RabbitMQ message was received (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageProcessed = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Information,
        new EventId(BaseEventId + 4, nameof(MessageProcessed)),
        "RabbitMQ message was processed (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _errorProcessMessage = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Error,
        new EventId(BaseEventId + 5, nameof(ErrorProcessMessage)),
        "RabbitMQ message processing failed (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    private static readonly Action<ILogger, string?, string?, Exception?> _queueCreated = LoggerMessage.Define<string?, string?>(
        LogLevel.Debug,
        new EventId(BaseEventId + 6, nameof(QueueCreated)),
        "RabbitMQ queue was created (Channel: {Channel}, NodeID: {NodeId})");

    private static readonly Action<ILogger, string?, string?, Exception?> _queueClosed = LoggerMessage.Define<string?, string?>(
        LogLevel.Debug,
        new EventId(BaseEventId + 7, nameof(QueueClosed)),
        "RabbitMQ queue was closed (Channel: {Channel}, NodeID: {NodeId})");

    private static readonly Action<ILogger, string?, string?, Exception?> _connectionCallbackError = LoggerMessage.Define<string?, string?>(
        LogLevel.Error,
        new EventId(BaseEventId + 8, nameof(ConnectionCallbackError)),
        "RabbitMQ connection callback failed (Channel: {Channel}, NodeID: {NodeId})");

    public static void SendMessage(this ILogger logger, string? messageId)
    {
        _sendMessage(logger, messageId, null);
    }

    public static void ErrorSendMessage(this ILogger logger, Exception exception)
    {
        _errorSendMessage(logger, exception);
    }

    public static void MessageReceived(this ILogger logger, string? channel, string? nodeId, string? messageId)
    {
        _messageReceived(logger, channel, nodeId, messageId, null);
    }

    public static void MessageProcessed(this ILogger logger, string? channel, string? nodeId, string? messageId)
    {
        _messageProcessed(logger, channel, nodeId, messageId, null);
    }

    public static void ErrorProcessMessage(this ILogger logger, Exception exception, string? channel, string? nodeId, string? messageId)
    {
        _errorProcessMessage(logger, channel, nodeId, messageId, exception);
    }

    public static void QueueCreated(this ILogger logger, string? channel, string? nodeId)
    {
        _queueCreated(logger, channel, nodeId, null);
    }

    public static void QueueClosed(this ILogger logger, string? channel, string? nodeId)
    {
        _queueClosed(logger, channel, nodeId, null);
    }

    public static void ConnectionCallbackError(this ILogger logger, Exception exception, string? channel, string? nodeId)
    {
        _connectionCallbackError(logger, channel, nodeId, exception);
    }
}
