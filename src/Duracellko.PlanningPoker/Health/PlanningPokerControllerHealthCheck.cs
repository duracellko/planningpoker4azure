using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duracellko.PlanningPoker.Health;

/// <summary>
/// The object provides health status of <see cref="Domain.IPlanningPoker"/> controller.
/// </summary>
public class PlanningPokerControllerHealthCheck : IHealthCheck
{
    private readonly IInitializationStatusProvider _initializationStatusProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerControllerHealthCheck"/> class.
    /// </summary>
    /// <param name="initializationStatusProvider">The service providing status of Planning Poker controller.</param>
    public PlanningPokerControllerHealthCheck(IInitializationStatusProvider initializationStatusProvider)
    {
        _initializationStatusProvider = initializationStatusProvider ?? throw new ArgumentNullException(nameof(initializationStatusProvider));
    }

    /// <summary>
    /// Runs the health check, returning the status of the Planning Poker controller.
    /// </summary>
    /// <param name="context">A context object associated with the current execution.</param>
    /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the health check.</param>
    /// <returns>The health status of the Planning Poker controller.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_initializationStatusProvider.IsInitialized)
        {
            return Task.FromResult(HealthCheckResult.Healthy(Resources.Health_PlanningPokerInitialized));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(Resources.Health_PlanningPokerInitializing));
        }
    }
}
