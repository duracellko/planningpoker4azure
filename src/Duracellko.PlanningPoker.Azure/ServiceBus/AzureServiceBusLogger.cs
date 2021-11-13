using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    internal static class AzureServiceBusLogger
    {
        private const int BaseEventId = 1600;

        private static readonly Action<ILogger, Exception?> _sendMessage = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(BaseEventId + 1, nameof(SendMessage)),
            "Message sent to Azure Service Bus.");

        private static readonly Action<ILogger, Exception?> _errorSendMessage = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(BaseEventId + 2, nameof(ErrorSendMessage)),
            "Error sending message to Azure Service Bus.");

        private static readonly Action<ILogger, string, string, Exception?> _subscriptionCreated = LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 3, nameof(SubscriptionCreated)),
            "Service Bus subscription was created (Topic: {Topic}, NodeID: {NodeId})");

        private static readonly Action<ILogger, string?, string?, Exception?> _subscriptionDeleted = LoggerMessage.Define<string?, string?>(
            LogLevel.Debug,
            new EventId(BaseEventId + 4, nameof(SubscriptionDeleted)),
            "Service Bus subscription was deleted (Topic: {Topic}, NodeID: {NodeId})");

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageReceived = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Debug,
            new EventId(BaseEventId + 5, nameof(MessageReceived)),
            "Service Bus message was received (Topic: {Topic}, NodeID: {NodeId}, MessageID: {MessageId})");

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _messageProcessed = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Information,
            new EventId(BaseEventId + 6, nameof(MessageProcessed)),
            "Service Bus message was processed (Topic: {Topic}, NodeID: {NodeId}, MessageID: {MessageId})");

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _errorProcessMessage = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            new EventId(BaseEventId + 7, nameof(ErrorProcessMessage)),
            "Service Bus message processing failed (Topic: {Topic}, NodeID: {NodeId}, MessageID: {MessageId})");

        private static readonly Action<ILogger, string?, string?, ServiceBusErrorSource, Exception?> _errorProcess = LoggerMessage.Define<string?, string?, ServiceBusErrorSource>(
            LogLevel.Error,
            new EventId(BaseEventId + 8, nameof(ErrorProcess)),
            "Service Bus processing failed (Topic: {Topic}, NodeID: {NodeId}, Error source: {ErrorSource})");

        private static readonly Action<ILogger, string?, Exception?> _errorSubscriptionsMaintenance = LoggerMessage.Define<string?>(
            LogLevel.Error,
            new EventId(BaseEventId + 9, nameof(ErrorSubscriptionsMaintenance)),
            "Service Bus subscriptions maintenance failed for Node ID: {NodeId}");

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _subscriptionAliveMessageReceived = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Debug,
            new EventId(BaseEventId + 10, nameof(SubscriptionAliveMessageReceived)),
            "Service Bus subscription is alive (Topic: {Topic}, NodeID: {NodeId}, SubscriptionID: {SubscriptionId})");

        private static readonly Action<ILogger, string?, Exception?> _subscriptionAliveSent = LoggerMessage.Define<string?>(
            LogLevel.Debug,
            new EventId(BaseEventId + 11, nameof(SubscriptionAliveSent)),
            "Notification sent, that Node ID \"{NodeId}\" is alive.");

        private static readonly Action<ILogger, string?, string?, Exception?> _inactiveSubscriptionDeleted = LoggerMessage.Define<string?, string?>(
            LogLevel.Debug,
            new EventId(BaseEventId + 12, nameof(InactiveSubscriptionDeleted)),
            "Service Bus subscription \"{SubscriptionId}\" was deleted due to inactivity by Node ID \"{NodeId}\".");

        private static readonly Action<ILogger, string?, string?, string, Exception?> _subscriptionDeleteFailed = LoggerMessage.Define<string?, string?, string>(
            LogLevel.Warning,
            new EventId(BaseEventId + 13, nameof(SubscriptionDeleteFailed)),
            "Deleting Service Bus subscription (Topic: {Topic}, NodeID: {SubscriptionId}) failed with error: {Error}");

        public static void SendMessage(this ILogger logger)
        {
            _sendMessage(logger, null);
        }

        public static void ErrorSendMessage(this ILogger logger, Exception exception)
        {
            _errorSendMessage(logger, exception);
        }

        public static void SubscriptionCreated(this ILogger logger, string topicName, string nodeId)
        {
            _subscriptionCreated(logger, topicName, nodeId, null);
        }

        public static void SubscriptionDeleted(this ILogger logger, string? topicName, string? nodeId)
        {
            _subscriptionDeleted(logger, topicName, nodeId, null);
        }

        public static void MessageReceived(this ILogger logger, string? topicName, string? nodeId, string? messageId)
        {
            _messageReceived(logger, topicName, nodeId, messageId, null);
        }

        public static void MessageProcessed(this ILogger logger, string? topicName, string? nodeId, string? messageId)
        {
            _messageProcessed(logger, topicName, nodeId, messageId, null);
        }

        public static void ErrorProcessMessage(this ILogger logger, Exception exception, string? topicName, string? nodeId, string? messageId)
        {
            _errorProcessMessage(logger, topicName, nodeId, messageId, exception);
        }

        public static void ErrorProcess(this ILogger logger, Exception exception, string? topicName, string? nodeId, ServiceBusErrorSource errorSource)
        {
            _errorProcess(logger, topicName, nodeId, errorSource, exception);
        }

        public static void ErrorSubscriptionsMaintenance(this ILogger logger, Exception exception, string? nodeId)
        {
            _errorSubscriptionsMaintenance(logger, nodeId, exception);
        }

        public static void SubscriptionAliveMessageReceived(this ILogger logger, string? topicName, string? nodeId, string? subscriptionId)
        {
            _subscriptionAliveMessageReceived(logger, topicName, nodeId, subscriptionId, null);
        }

        public static void SubscriptionAliveSent(this ILogger logger, string? nodeId)
        {
            _subscriptionAliveSent(logger, nodeId, null);
        }

        public static void InactiveSubscriptionDeleted(this ILogger logger, string? nodeId, string? subscriptionId)
        {
            _inactiveSubscriptionDeleted(logger, subscriptionId, nodeId, null);
        }

        public static void SubscriptionDeleteFailed(this ILogger logger, Exception exception, string? topicName, string? subscriptionId)
        {
            _subscriptionDeleteFailed(logger, topicName, subscriptionId, exception.Message, null);
        }
    }
}
