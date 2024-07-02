using Duracellko.PlanningPoker.Web.Model;

namespace Duracellko.PlanningPoker.Web;

public class PlanningPokerClientConfiguration
{
    public ServerSideConditions UseServerSide { get; set; }

    public bool UseHttpClient { get; set; }
}
