using System;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Information about team member displayed in list of members.
    /// </summary>
    public class MemberItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberItem" /> class.
        /// </summary>
        /// <param name="member">The member used to initialize the item.</param>
        /// <param name="missingEstimation">Value indicating whether the member has missing estimation.</param>
        public MemberItem(TeamMember member, bool missingEstimation)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Name = member.Name;
            MissingEstimation = missingEstimation;
        }

        /// <summary>
        /// Gets a name of the member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether final estimation is waiting
        /// for the member to select estimation.
        /// </summary>
        public bool MissingEstimation { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }
}
