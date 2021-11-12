using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game.
    /// </summary>
    public class TeamMember
    {
        /// <summary>
        /// Gets or sets the type of member. Value can be one of ScrumMaster, Member or Observer.
        /// </summary>
        /// <value>
        /// The type of member.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Provides type of member for javascript.")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the member's name.
        /// </summary>
        /// <value>The member's name.</value>
        public string Name { get; set; } = string.Empty;
    }
}
