namespace Duracellko.PlanningPoker.Domain.Test
{
    public static class TestHelper
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
