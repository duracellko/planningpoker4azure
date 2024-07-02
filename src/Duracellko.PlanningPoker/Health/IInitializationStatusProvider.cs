namespace Duracellko.PlanningPoker.Health;

/// <summary>
/// Provides status about initialization of a service or a controller.
/// </summary>
public interface IInitializationStatusProvider
{
    /// <summary>
    /// Gets a value indicating whether implemented service is initialized.
    /// </summary>
    bool IsInitialized { get; }
}
