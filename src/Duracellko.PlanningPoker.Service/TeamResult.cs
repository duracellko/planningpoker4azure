using System;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Result data of creating or joining Scrum Team.
    /// </summary>
    public class TeamResult
    {
        /// <summary>
        /// Gets or sets the Scrum team.
        /// </summary>
        /// <value>
        /// The Scrum team.
        /// </value>
        public ScrumTeam? ScrumTeam { get; set; }

        /// <summary>
        /// Gets or sets the session ID to use to get messages.
        /// </summary>
        /// <value>
        /// The session ID.
        /// </value>
        public Guid SessionId { get; set; }
    }
}
