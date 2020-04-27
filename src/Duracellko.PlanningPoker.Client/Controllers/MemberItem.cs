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
        /// <param name="hasEstimated">Value indicating whether the member has estimation.</param>
        public MemberItem(TeamMember member, bool hasEstimated)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Name = member.Name;
            HasEstimated = hasEstimated;
        }

        /// <summary>
        /// Gets a name of the member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether member has already estimated.
        /// </summary>
        public bool HasEstimated { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }
}
