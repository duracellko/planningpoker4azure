using System;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Azure;

internal static class PlanningPokerAzureNodeLogger
{
    private const int BaseEventId = 1500;

    private static readonly Action<ILogger, string, Exception?> _planningPokerAzureNodeStarting = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(BaseEventId + 1, nameof(PlanningPokerAzureNodeStarting)),
        "Planning Poker Azure Node \"{NodeId}\" is starting.");

    private static readonly Action<ILogger, string, Exception?> _planningPokerAzureNodeStopping = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(BaseEventId + 2, nameof(PlanningPokerAzureNodeStopping)),
        "Planning Poker Azure Node \"{NodeId}\" is stopping.");

    private static readonly Action<ILogger, string, string, Exception?> _errorCreateTeamNodeMessage = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(BaseEventId + 3, nameof(ErrorCreateTeamNodeMessage)),
        "Error creating TeamCreated node message for team \"{TeamName}\". (NodeID: {NodeId})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, Exception?> _nodeMessageSent = LoggerMessage.Define<string, string?, string?, NodeMessageType>(
        LogLevel.Information,
        new EventId(BaseEventId + 4, nameof(NodeMessageSent)),
        "Planning Poker Azure Node message sent (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, Exception?> _errorSendingNodeMessage = LoggerMessage.Define<string, string?, string?, NodeMessageType>(
        LogLevel.Error,
        new EventId(BaseEventId + 5, nameof(ErrorSendingNodeMessage)),
        "Error sending Planning Poker Azure Node message (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, string, Exception?> _scrumTeamCreatedNodeMessageReceived = LoggerMessage.Define<string, string?, string?, NodeMessageType, string>(
        LogLevel.Information,
        new EventId(BaseEventId + 6, nameof(ScrumTeamCreatedNodeMessageReceived)),
        "Planning Poker Azure Node message received (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type}, Scrum Team: {ScrumTeam})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, Exception?> _errorScrumTeamCreatedNodeMessage = LoggerMessage.Define<string, string?, string?, NodeMessageType>(
        LogLevel.Error,
        new EventId(BaseEventId + 7, nameof(ErrorScrumTeamCreatedNodeMessage)),
        "Error processing Planning Poker Azure Node message (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, string?, MessageType, Exception?> _scrumTeamNodeMessageReceived = LoggerMessage.Define<string, string?, string?, NodeMessageType, string?, MessageType>(
        LogLevel.Information,
        new EventId(BaseEventId + 8, nameof(ScrumTeamNodeMessageReceived)),
        "Planning Poker Azure Node message received (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type}, Scrum Team: {ScrumTeam}, Message type: {MessageType})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, string?, MessageType, Exception?> _errorProcessingScrumTeamNodeMessage = LoggerMessage.Define<string, string?, string?, NodeMessageType, string?, MessageType>(
        LogLevel.Error,
        new EventId(BaseEventId + 9, nameof(ErrorProcessingScrumTeamNodeMessage)),
        "Error processing Planning Poker Azure Node message (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type}, Scrum Team: {ScrumTeam}, Message type: {MessageType})");

    private static readonly Action<ILogger, string, string?, string?, NodeMessageType, Exception?> _nodeMessageReceived = LoggerMessage.Define<string, string?, string?, NodeMessageType>(
        LogLevel.Information,
        new EventId(BaseEventId + 10, nameof(NodeMessageReceived)),
        "Planning Poker Azure Node message received (NodeID: {NodeId}, Sender: {Sender}, Recipient: {Recipient}, Type: {Type})");

    private static readonly Action<ILogger, string, Exception?> _retryRequestTeamList = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(BaseEventId + 11, nameof(RetryRequestTeamList)),
        "Retry to request Scrum team list for Node \"{NodeId}\".");

    private static readonly Action<ILogger, string, Exception?> _planningPokerAzureNodeInitialized = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(BaseEventId + 12, nameof(PlanningPokerAzureNodeInitialized)),
        "Planning Poker Azure Node \"{NodeId}\" is initialized.");

    public static void PlanningPokerAzureNodeStarting(this ILogger logger, string nodeId)
    {
        _planningPokerAzureNodeStarting(logger, nodeId, null);
    }

    public static void PlanningPokerAzureNodeStopping(this ILogger logger, string nodeId)
    {
        _planningPokerAzureNodeStopping(logger, nodeId, null);
    }

    public static void ErrorCreateTeamNodeMessage(this ILogger logger, Exception exception, string nodeId, string teamName)
    {
        _errorCreateTeamNodeMessage(logger, teamName, nodeId, exception);
    }

    public static void NodeMessageSent(this ILogger logger, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type)
    {
        _nodeMessageSent(logger, nodeId, senderNodeId, recipientNodeId, type, null);
    }

    public static void ErrorSendingNodeMessage(this ILogger logger, Exception exception, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type)
    {
        _errorSendingNodeMessage(logger, nodeId, senderNodeId, recipientNodeId, type, exception);
    }

    public static void ScrumTeamCreatedNodeMessageReceived(this ILogger logger, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type, string scrumTeam)
    {
        _scrumTeamCreatedNodeMessageReceived(logger, nodeId, senderNodeId, recipientNodeId, type, scrumTeam, null);
    }

    public static void ErrorScrumTeamCreatedNodeMessage(this ILogger logger, Exception exception, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type)
    {
        _errorScrumTeamCreatedNodeMessage(logger, nodeId, senderNodeId, recipientNodeId, type, exception);
    }

    public static void ScrumTeamNodeMessageReceived(this ILogger logger, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type, string? scrumTeam, MessageType messageType)
    {
        _scrumTeamNodeMessageReceived(logger, nodeId, senderNodeId, recipientNodeId, type, scrumTeam, messageType, null);
    }

    public static void ErrorProcessingScrumTeamNodeMessage(this ILogger logger, Exception exception, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type, string? scrumTeam, MessageType messageType)
    {
        _errorProcessingScrumTeamNodeMessage(logger, nodeId, senderNodeId, recipientNodeId, type, scrumTeam, messageType, exception);
    }

    public static void NodeMessageReceived(this ILogger logger, string nodeId, string? senderNodeId, string? recipientNodeId, NodeMessageType type)
    {
        _nodeMessageReceived(logger, nodeId, senderNodeId, recipientNodeId, type, null);
    }

    public static void RetryRequestTeamList(this ILogger logger, string nodeId)
    {
        _retryRequestTeamList(logger, nodeId, null);
    }

    public static void PlanningPokerAzureNodeInitialized(this ILogger logger, string nodeId)
    {
        _planningPokerAzureNodeInitialized(logger, nodeId, null);
    }
}
