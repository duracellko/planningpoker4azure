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
    }
}
