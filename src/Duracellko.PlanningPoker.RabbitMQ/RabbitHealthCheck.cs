using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duracellko.PlanningPoker.RabbitMQ;

/// <summary>
/// Health check that reports status of RabbitMQ connection.
/// </summary>
public class RabbitHealthCheck : IHealthCheck
{
    private readonly RabbitServiceBus _rabbitServiceBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitHealthCheck"/> class.
    /// </summary>
    /// <param name="rabbitServiceBus">The service that manages connection to RabbitMQ.</param>
    public RabbitHealthCheck(RabbitServiceBus rabbitServiceBus)
    {
        ArgumentNullException.ThrowIfNull(rabbitServiceBus);
        _rabbitServiceBus = rabbitServiceBus;
    }

    /// <summary>
    /// Runs the health check, returning the status of the RabbitMQ connection.
    /// </summary>
    /// <param name="context">A context object associated with the current execution.</param>
    /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the health check.</param>
    /// <returns>The health status of the RabbitMQ connection.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthResult = _rabbitServiceBus.IsConnected ?
            HealthCheckResult.Healthy(Resources.Health_RabbitHealthy) :
            HealthCheckResult.Unhealthy(Resources.Health_RabbitUnhealthy);
        return Task.FromResult(healthResult);
    }
}
