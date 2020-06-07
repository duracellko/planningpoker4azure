using System.Collections.Generic;

namespace Duracellko.PlanningPoker.Domain.Test
{
    internal static class ScrumTeamTestData
    {
        public static ScrumTeam CreateScrumTeam(
            string name,
            IEnumerable<Estimation> availableEstimations = null,
            DateTimeProvider dateTimeProvider = null,
            GuidProvider guidProvider = null)
        {
            return new ScrumTeam(name, availableEstimations, dateTimeProvider, guidProvider);
        }

        public static IEnumerable<Estimation> GetCustomEstimationDeck()
        {
            return new Estimation[]
            {
                new Estimation(99),
                new Estimation(-1),
                new Estimation(),
                new Estimation(22.34),
                new Estimation(-100.2)
            };
        }
    }
}
