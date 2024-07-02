using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duracellko.PlanningPoker.Azure.Health;

/// <summary>
/// The object provides health status of Azure Service Bus subscription.
/// </summary>
public class AzureServiceBusHealthCheck : IHealthCheck
{
    private static readonly CompositeFormat _healthAzureServiceBusHealthy = CompositeFormat.Parse(Resources.Health_AzureServiceBusHealthy);

    private readonly PlanningPokerAzureNode _node;
    private readonly Lazy<ServiceBusAdministrationClient> _serviceBusAdministrationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBusHealthCheck"/> class.
    /// </summary>
    /// <param name="node">The Planning Poker Azure Node to provide the health check for.</param>
    public AzureServiceBusHealthCheck(PlanningPokerAzureNode node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _serviceBusAdministrationClient = new Lazy<ServiceBusAdministrationClient>(
            () => new ServiceBusAdministrationClient(_node.Configuration.ServiceBusConnectionString));
    }

    /// <summary>
    /// Runs the health check, returning the status of the Azure Service Bus subscription.
    /// </summary>
    /// <param name="context">A context object associated with the current execution.</param>
    /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the health check.</param>
    /// <returns>The health status of the Azure Service Bus subscription.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All errors are reported as unhealthy status.")]
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = _node.Configuration;
            var topicName = configuration.ServiceBusTopic;
            if (string.IsNullOrEmpty(topicName))
            {
                topicName = "PlanningPoker";
            }

            var properties = await _serviceBusAdministrationClient.Value.GetSubscriptionRuntimePropertiesAsync(topicName, _node.NodeId, cancellationToken);
            return HealthCheckResult.Healthy(string.Format(CultureInfo.InvariantCulture, _healthAzureServiceBusHealthy, properties.Value.ActiveMessageCount));
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(Resources.Health_AzureServiceBusUnhealthy, ex);
        }
    }
}
