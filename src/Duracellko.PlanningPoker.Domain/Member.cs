using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Fields are placed near properties.")]
    public class Member : Observer
    {
        private Estimation _estimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the member is joining.</param>
        /// <param name="name">The member name.</param>
        public Member(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        /// <param name="team">The Scrum team the observer is joining.</param>
        /// <param name="memberData">The member serialization data.</param>
        internal Member(ScrumTeam team, Serialization.MemberData memberData)
            : base(team, memberData)
        {
            _estimation = memberData.Estimation;
        }

        /// <summary>
        /// Gets or sets the estimation, the member is picking in planning poker.
        /// </summary>
        /// <value>
        /// The estimation.
        /// </value>
        public Estimation Estimation
        {
            get
            {
                return _estimation;
            }

            set
            {
                if (_estimation != value)
                {
                    if (value != null && !Team.AvailableEstimations.Contains(value))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Error_EstimationIsNotAvailableInTeam, value.Value), nameof(value));
                    }

                    _estimation = value;
                    if (Team.State == TeamState.EstimationInProgress)
                    {
                        Team.OnMemberEstimated(this);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the estimation to unselected.
        /// </summary>
        internal void ResetEstimation()
        {
            _estimation = null;
        }
    }
}
