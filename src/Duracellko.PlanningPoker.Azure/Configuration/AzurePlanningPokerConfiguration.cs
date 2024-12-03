using System;
using Duracellko.PlanningPoker.Configuration;

namespace Duracellko.PlanningPoker.Azure.Configuration;

/// <summary>
/// Configuration section of planning poker for Azure platform.
/// </summary>
public class AzurePlanningPokerConfiguration : PlanningPokerConfiguration, IAzurePlanningPokerConfiguration
{
    /// <summary>
    /// Gets or sets a connection string to Azure ServiceBus.
    /// </summary>
    /// <value>ServiceBus connection string.</value>
    public string? ServiceBusConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a topic for communication on Azure ServiceBus.
    /// </summary>
    /// <value>ServiceBus topic name.</value>
    public string? ServiceBusTopic { get; set; }

    /// <summary>
    /// Gets or sets a time in seconds to wait for end of initialization phase.
    /// </summary>
    /// <value>The initialization wait time.</value>
    public int InitializationTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets a time in seconds to wait for any message in initialization phase.
    /// </summary>
    /// <value>The initialization message wait time.</value>
    public int InitializationMessageTimeout { get; set; } = 5;

    /// <summary>
    /// Gets or sets a time interval in seconds, when a planning poker node notifies other nodes about its activity and checks for inactive subscriptions.
    /// </summary>
    /// <value>The subscription maintenance time interval.</value>
    public int SubscriptionMaintenanceInterval { get; set; } = 300;

    /// <summary>
    /// Gets or sets a time in seconds that an inactive subscription is deleted after.
    /// </summary>
    /// <value>The subscription inactivity time.</value>
    public int SubscriptionInactivityTimeout { get; set; } = 900;

    TimeSpan IAzurePlanningPokerConfiguration.InitializationTimeout => TimeSpan.FromSeconds(InitializationTimeout);

    TimeSpan IAzurePlanningPokerConfiguration.InitializationMessageTimeout => TimeSpan.FromSeconds(InitializationMessageTimeout);

    TimeSpan IAzurePlanningPokerConfiguration.SubscriptionMaintenanceInterval => TimeSpan.FromSeconds(SubscriptionMaintenanceInterval);

    TimeSpan IAzurePlanningPokerConfiguration.SubscriptionInactivityTimeout => TimeSpan.FromSeconds(SubscriptionInactivityTimeout);
}
