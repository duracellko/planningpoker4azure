using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game.
    /// </summary>
    [Serializable]
    public class TeamMember
    {
        /// <summary>
        /// Gets or sets the type of member. Value can be one of ScrumMaster, Member or Observer.
        /// </summary>
        /// <value>
        /// The type of member.
        /// </value>
        [JsonProperty("type")]
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Provides type of member for javascript.")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the member's name.
        /// </summary>
        /// <value>The member's name.</value>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
