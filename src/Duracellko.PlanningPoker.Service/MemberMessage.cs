using System;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Message sent to other members of Scrum team, when a new member joins the team or someone disconnects the planning poker.
    /// </summary>
    [Serializable]
    public class MemberMessage : Message
    {
        /// <summary>
        /// Gets or sets the Scrum team member, who joined or disconnected.
        /// </summary>
        /// <value>
        /// The team member.
        /// </value>
        [JsonProperty("member")]
        public TeamMember Member { get; set; }
    }
}
