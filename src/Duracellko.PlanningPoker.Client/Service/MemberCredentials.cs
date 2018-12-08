namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Information about team name and member name that user is connected to.
    /// </summary>
    public class MemberCredentials
    {
        /// <summary>
        /// Gets or sets a name of team.
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// Gets or sets a name of member.
        /// </summary>
        public string MemberName { get; set; }
    }
}
