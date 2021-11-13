using System;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Controllers
{
    internal static class PlanningPokerControllerLogger
    {
        private const int BaseEventId = 1000;

        private static readonly Action<ILogger, string, string, Exception?> _scrumTeamCreated = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(BaseEventId + 1, nameof(ScrumTeamCreated)),
            "Scrum team \"{ScrumTeam}\" was created with Scrum master: {ScrumMaster}");

        private static readonly Action<ILogger, string, Exception?> _scrumTeamAttached = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(BaseEventId + 2, nameof(ScrumTeamAttached)),
            "Scrum team \"{ScrumTeam}\" was attached.");

        private static readonly Action<ILogger, string, Exception?> _readScrumTeam = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 3, nameof(ReadScrumTeam)),
            "Scrum team \"{ScrumTeam}\" was locked.");

        private static readonly Action<ILogger, string, string, bool, Exception?> _observerMessageReceived = LoggerMessage.Define<string, string, bool>(
            LogLevel.Debug,
            new EventId(BaseEventId + 4, nameof(ObserverMessageReceived)),
            "Observer \"{Observer}\" in Scrum team \"{ScrumTeam}\" received message: {HasMessages}");

        private static readonly Action<ILogger, string, Exception?> _disconnectingInactiveObservers = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(BaseEventId + 5, nameof(DisconnectingInactiveObservers)),
            "Disconnecting inactive observers in Scrum team: {ScrumTeam}");

        private static readonly Action<ILogger, string, Exception?> _debugScrumTeamAdded = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 6, nameof(DebugScrumTeamAdded)),
            "Scrum team \"{ScrumTeam}\" was added.");

        private static readonly Action<ILogger, string, Exception?> _debugScrumTeamRemoved = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 7, nameof(DebugScrumTeamRemoved)),
            "Scrum team \"{ScrumTeam}\" was removed.");

        private static readonly Action<ILogger, string, Exception?> _scrumTeamRemoved = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(BaseEventId + 8, nameof(ScrumTeamRemoved)),
            "Scrum team \"{ScrumTeam}\" was removed.");

        private static readonly Action<ILogger, string, long, MessageType, Exception?> _scrumTeamMessage = LoggerMessage.Define<string, long, MessageType>(
            LogLevel.Information,
            new EventId(BaseEventId + 9, nameof(ScrumTeamMessage)),
            "Scrum team message (team: {ScrumTeam}, ID: {MessageId}, type: {MessageType})");

        private static readonly Action<ILogger, string, long, MessageType, string?, Exception?> _memberMessage = LoggerMessage.Define<string, long, MessageType, string?>(
            LogLevel.Information,
            new EventId(BaseEventId + 10, nameof(MemberMessage)),
            "Scrum team member message (team: {ScrumTeam}, ID: {MessageId}, type: {MessageType}, member: {Observer})");

        public static void ScrumTeamCreated(this ILogger logger, string scrumTeam, string scrumMaster)
        {
            _scrumTeamCreated(logger, scrumTeam, scrumMaster, null);
        }

        public static void ScrumTeamAttached(this ILogger logger, string scrumTeam)
        {
            _scrumTeamAttached(logger, scrumTeam, null);
        }

        public static void ReadScrumTeam(this ILogger logger, string scrumTeam)
        {
            _readScrumTeam(logger, scrumTeam, null);
        }

        public static void ObserverMessageReceived(this ILogger logger, string scrumTeam, string observer, bool hasMessages)
        {
            _observerMessageReceived(logger, scrumTeam, observer, hasMessages, null);
        }

        public static void DisconnectingInactiveObservers(this ILogger logger, string scrumTeam)
        {
            _disconnectingInactiveObservers(logger, scrumTeam, null);
        }

        public static void DebugScrumTeamAdded(this ILogger logger, string scrumTeam)
        {
            _debugScrumTeamAdded(logger, scrumTeam, null);
        }

        public static void DebugScrumTeamRemoved(this ILogger logger, string scrumTeam)
        {
            _debugScrumTeamRemoved(logger, scrumTeam, null);
        }

        public static void ScrumTeamRemoved(this ILogger logger, string scrumTeam)
        {
            _scrumTeamRemoved(logger, scrumTeam, null);
        }

        public static void ScrumTeamMessage(this ILogger logger, string scrumTeam, long messageId, MessageType messageType)
        {
            _scrumTeamMessage(logger, scrumTeam, messageId, messageType, null);
        }

        public static void MemberMessage(this ILogger logger, string scrumTeam, long messageId, MessageType messageType, string? observer)
        {
            _memberMessage(logger, scrumTeam, messageId, messageType, observer, null);
        }
    }
}
