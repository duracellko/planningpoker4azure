using System.Collections.Generic;

namespace Duracellko.PlanningPoker.Domain.Test
{
    internal static class TestHelper
    {
        public static void ClearMessages(Observer observer)
        {
            while (observer.HasMessage)
            {
                observer.PopMessage();
            }
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
