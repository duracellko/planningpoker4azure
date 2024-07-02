using System;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Redis;

internal static class RedisServiceBusLogger
{
    private const int BaseEventId = 1700;

    private static readonly Action<ILogger, Exception?> _sendMessage = LoggerMessage.Define(
        LogLevel.Debug,
        new EventId(BaseEventId + 1, nameof(SendMessage)),
        "Message sent to Redis.");

    private static readonly Action<ILogger, Exception?> _errorSendMessage = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(BaseEventId + 2, nameof(ErrorSendMessage)),
        "Error sending message to Redis.");

    private static readonly Action<ILogger, string, string, Exception?> _subscriptionCreated = LoggerMessage.Define<string, string>(
        LogLevel.Debug,
        new EventId(BaseEventId + 3, nameof(SubscriptionCreated)),
        "Redis subscription was created (Channel: {Channel}, NodeID: {NodeId})");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageReceived = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Debug,
        new EventId(BaseEventId + 5, nameof(MessageReceived)),
        "Redis message was received (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageProcessed = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Information,
        new EventId(BaseEventId + 6, nameof(MessageProcessed)),
        "Redis message was processed (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    private static readonly Action<ILogger, string?, string?, string?, Exception?> _errorProcessMessage = LoggerMessage.Define<string?, string?, string?>(
        LogLevel.Error,
        new EventId(BaseEventId + 7, nameof(ErrorProcessMessage)),
        "Redis message processing failed (Channel: {Channel}, NodeID: {NodeId}, MessageID: {MessageId})");

    public static void SendMessage(this ILogger logger)
    {
        _sendMessage(logger, null);
    }

    public static void ErrorSendMessage(this ILogger logger, Exception exception)
    {
        _errorSendMessage(logger, exception);
    }

    public static void SubscriptionCreated(this ILogger logger, string channel, string nodeId)
    {
        _subscriptionCreated(logger, channel, nodeId, null);
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
}
