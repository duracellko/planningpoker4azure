using System.Collections.Generic;

namespace Duracellko.PlanningPoker.Domain.Test;

internal static class ScrumTeamTestData
{
    public static ScrumTeam CreateScrumTeam(
        string name,
        IEnumerable<Estimation>? availableEstimations = null,
        DateTimeProvider? dateTimeProvider = null,
        GuidProvider? guidProvider = null)
    {
        return new ScrumTeam(name, availableEstimations, dateTimeProvider, guidProvider);
    }

    public static IEnumerable<Estimation> GetCustomEstimationDeck()
    {
        return
        [
            new(99),
            new(-1),
            new(),
            new(22.34),
            new(-100.2)
        ];
    }
}
