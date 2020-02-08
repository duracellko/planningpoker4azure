using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum master can additionally to member start and cancel estimation planning poker.
    /// </summary>
    public class ScrumMaster : Member
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumMaster"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the the master is joining.</param>
        /// <param name="name">The Scrum master name.</param>
        public ScrumMaster(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumMaster"/> class.
        /// </summary>
        /// <param name="team">The Scrum team the observer is joining.</param>
        /// <param name="memberData">The member serialization data.</param>
        internal ScrumMaster(ScrumTeam team, Serialization.MemberData memberData)
            : base(team, memberData)
        {
        }

        /// <summary>
        /// Starts new estimation.
        /// </summary>
        public void StartEstimation()
        {
            if (Team.State == TeamState.EstimationInProgress)
            {
                throw new InvalidOperationException(Resources.Error_EstimationIsInProgress);
            }

            Team.StartEstimation();
        }

        /// <summary>
        /// Cancels current estimation.
        /// </summary>
        public void CancelEstimation()
        {
            if (Team.State == TeamState.EstimationInProgress)
            {
                Team.CancelEstimation();
            }
        }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        protected internal override Serialization.MemberData GetData()
        {
            var result = base.GetData();
            result.MemberType = Serialization.MemberType.ScrumMaster;
            return result;
        }
    }
}
