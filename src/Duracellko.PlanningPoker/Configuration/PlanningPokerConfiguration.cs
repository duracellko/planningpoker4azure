using System;

namespace Duracellko.PlanningPoker.Configuration;

/// <summary>
/// Configuration section for planning poker.
/// </summary>
public class PlanningPokerConfiguration : IPlanningPokerConfiguration
{
    /// <summary>
    /// Gets or sets the time in seconds, after which is client disconnected when he/she does not check for new messages.
    /// </summary>
    /// <value>The client inactivity timeout.</value>
    public int ClientInactivityTimeout { get; set; } = 900;

    /// <summary>
    /// Gets or sets the interval in seconds, how often is executed job to search for and disconnect inactive clients.
    /// </summary>
    /// <value>The client inactivity check interval.</value>
    public int ClientInactivityCheckInterval { get; set; } = 60;

    /// <summary>
    /// Gets or sets the time, how long can client wait for new message. Empty message collection is sent to the client after the specified time.
    /// </summary>
    /// <value>The wait for message timeout.</value>
    public int WaitForMessageTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets the repository folder to store scrum teams.
    /// </summary>
    /// <value>
    /// The repository folder.
    /// </value>
    public string? RepositoryFolder { get; set; }

    /// <summary>
    /// Gets or sets the time in seconds, after which team (file) is deleted from repository when it is not used.
    /// </summary>
    /// <value>The repository file expiration time.</value>
    public int RepositoryTeamExpiration { get; set; } = 1200;

    TimeSpan IPlanningPokerConfiguration.ClientInactivityTimeout
    {
        get { return TimeSpan.FromSeconds(ClientInactivityTimeout); }
    }

    TimeSpan IPlanningPokerConfiguration.ClientInactivityCheckInterval
    {
        get { return TimeSpan.FromSeconds(ClientInactivityCheckInterval); }
    }

    TimeSpan IPlanningPokerConfiguration.WaitForMessageTimeout
    {
        get { return TimeSpan.FromSeconds(WaitForMessageTimeout); }
    }

    TimeSpan IPlanningPokerConfiguration.RepositoryTeamExpiration
    {
        get { return TimeSpan.FromSeconds(RepositoryTeamExpiration); }
    }
}
