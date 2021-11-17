using System;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Service
{
    internal static class PlanningPokerHubLogger
    {
        private const int BaseEventId = 1100;

        private static readonly Action<ILogger, string, string, string, Deck, Exception?> _createTeam = LoggerMessage.Define<string, string, string, Deck>(
            LogLevel.Information,
            new EventId(BaseEventId + 1, nameof(CreateTeam)),
            "{Action}(\"{TeamName}\", \"{ScrumMasterName}\", {Deck})");

        private static readonly Action<ILogger, string, string, string, bool, Exception?> _joinTeam = LoggerMessage.Define<string, string, string, bool>(
            LogLevel.Information,
            new EventId(BaseEventId + 2, nameof(JoinTeam)),
            "{Action}(\"{TeamName}\", \"{MemberName}\", {AsObserver})");

        private static readonly Action<ILogger, string, string, string, Exception?> _reconnectTeam = LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 3, nameof(ReconnectTeam)),
            "{Action}(\"{TeamName}\", \"{MemberName}\")");

        private static readonly Action<ILogger, string, string, string, Exception?> _disconnectTeam = LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 4, nameof(DisconnectTeam)),
            "{Action}(\"{TeamName}\", \"{MemberName}\")");

        private static readonly Action<ILogger, string, string, Exception?> _startEstimation = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 5, nameof(StartEstimation)),
            "{Action}(\"{TeamName}\")");

        private static readonly Action<ILogger, string, string, Exception?> _cancelEstimation = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 6, nameof(CancelEstimation)),
            "{Action}(\"{TeamName}\")");

        private static readonly Action<ILogger, string, string, string, double?, Exception?> _submitEstimation = LoggerMessage.Define<string, string, string, double?>(
            LogLevel.Information,
            new EventId(BaseEventId + 7, nameof(SubmitEstimation)),
            "{Action}(\"{TeamName}\", \"{MemberName}\", {Estimation})");

        private static readonly Action<ILogger, string, string, string, TimeSpan, Exception?> _startTimer = LoggerMessage.Define<string, string, string, TimeSpan>(
            LogLevel.Information,
            new EventId(BaseEventId + 10, nameof(StartTimer)),
            "{Action}(\"{TeamName}\", \"{MemberName}\", {Duration})");

        private static readonly Action<ILogger, string, string, string, Exception?> _cancelTimer = LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 11, nameof(CancelTimer)),
            "{Action}(\"{TeamName}\", \"{MemberName}\")");

        private static readonly Action<ILogger, string, string, string, Guid, long, Exception?> _getMessages = LoggerMessage.Define<string, string, string, Guid, long>(
            LogLevel.Information,
            new EventId(BaseEventId + 8, nameof(GetMessages)),
            "{Action}(\"{TeamName}\", \"{MemberName}\", {SessionId}, {LastMessageId})");

        private static readonly Action<ILogger, string, Exception?> _messageReceived = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 9, nameof(MessageReceived)),
            "Notify messages received (connectionId: {ConnectionId})");

        public static void CreateTeam(this ILogger logger, string teamName, string scrumMasterName, Deck deck)
        {
            _createTeam(logger, nameof(CreateTeam), teamName, scrumMasterName, deck, null);
        }

        public static void JoinTeam(this ILogger logger, string teamName, string memberName, bool asObserver)
        {
            _joinTeam(logger, nameof(JoinTeam), teamName, memberName, asObserver, null);
        }

        public static void ReconnectTeam(this ILogger logger, string teamName, string memberName)
        {
            _reconnectTeam(logger, nameof(ReconnectTeam), teamName, memberName, null);
        }

        public static void DisconnectTeam(this ILogger logger, string teamName, string memberName)
        {
            _disconnectTeam(logger, nameof(DisconnectTeam), teamName, memberName, null);
        }

        public static void StartEstimation(this ILogger logger, string teamName)
        {
            _startEstimation(logger, nameof(StartEstimation), teamName, null);
        }

        public static void CancelEstimation(this ILogger logger, string teamName)
        {
            _cancelEstimation(logger, nameof(CancelEstimation), teamName, null);
        }

        public static void SubmitEstimation(this ILogger logger, string teamName, string memberName, double? estimation)
        {
            _submitEstimation(logger, nameof(SubmitEstimation), teamName, memberName, estimation, null);
        }

        public static void StartTimer(this ILogger logger, string teamName, string memberName, TimeSpan duration)
        {
            _startTimer(logger, nameof(StartTimer), teamName, memberName, duration, null);
        }

        public static void CancelTimer(this ILogger logger, string teamName, string memberName)
        {
            _cancelTimer(logger, nameof(CancelTimer), teamName, memberName, null);
        }

        public static void GetMessages(this ILogger logger, string teamName, string memberName, Guid sessionId, long lastMessageId)
        {
            _getMessages(logger, nameof(GetMessages), teamName, memberName, sessionId, lastMessageId, null);
        }

        public static void MessageReceived(this ILogger logger, string connectionId)
        {
            _messageReceived(logger, connectionId, null);
        }
    }
}
