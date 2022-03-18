namespace Duracellko.PlanningPoker.Health
{
    /// <summary>
    /// Provides initialization status about a service that is always initialized.
    /// </summary>
    public class ReadyInitializationStatusProvider : IInitializationStatusProvider
    {
        /// <summary>
        /// Gets a value indicating whether implemented service is initialized.
        /// The value is always <c>true</c>.
        /// </summary>
        public bool IsInitialized => true;
    }
}
