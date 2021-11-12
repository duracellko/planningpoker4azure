using System;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Data
{
    internal static class FileScrumTeamRepositoryLogger
    {
        private const int BaseEventId = 1050;

        private static readonly Action<ILogger, Exception> _loadScrumTeamNames = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(BaseEventId + 1, nameof(LoadScrumTeamNames)),
            "Loading Scrum team names.");

        private static readonly Action<ILogger, string, Exception> _loadScrumTeam = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 2, nameof(LoadScrumTeam)),
            "Loaded Scrum team: {ScrumTeam}");

        private static readonly Action<ILogger, string, Exception> _saveScrumTeam = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 3, nameof(SaveScrumTeam)),
            "Saved Scrum team: {ScrumTeam}");

        private static readonly Action<ILogger, string, Exception> _deleteScrumTeam = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(BaseEventId + 4, nameof(DeleteScrumTeam)),
            "Deleted Scrum team: {ScrumTeam}");

        private static readonly Action<ILogger, Exception> _deleteExpiredScrumTeams = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(BaseEventId + 5, nameof(DeleteExpiredScrumTeams)),
            "Deleting expired Scrum teams.");

        private static readonly Action<ILogger, Exception> _deleteAll = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(BaseEventId + 6, nameof(DeleteAll)),
            "Deleting all Scrum teams.");

        public static void LoadScrumTeamNames(this ILogger logger)
        {
            _loadScrumTeamNames(logger, null);
        }

        public static void LoadScrumTeam(this ILogger logger, string scrumTeam)
        {
            _loadScrumTeam(logger, scrumTeam, null);
        }

        public static void SaveScrumTeam(this ILogger logger, string scrumTeam)
        {
            _saveScrumTeam(logger, scrumTeam, null);
        }

        public static void DeleteScrumTeam(this ILogger logger, string scrumTeam)
        {
            _deleteScrumTeam(logger, scrumTeam, null);
        }

        public static void DeleteExpiredScrumTeams(this ILogger logger)
        {
            _deleteExpiredScrumTeams(logger, null);
        }

        public static void DeleteAll(this ILogger logger)
        {
            _deleteAll(logger, null);
        }
    }
}
