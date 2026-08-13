using Duracellko.PlanningPoker.Web.Model;

namespace Duracellko.PlanningPoker.Web;

public class PlanningPokerClientConfiguration
{
    public ApplicationMode ApplicationMode { get; set; }

    public bool UseHttpClient { get; set; }

    /// <summary>
    /// Gets or sets the timeout in milliseconds for loading the Planning Poker client.
    /// </summary>
    public int ClientLoadingTimeout { get; set; }
}
