// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Fields are placed near properties.")]
    public class Member : Observer
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the member is joining.</param>
        /// <param name="name">The member name.</param>
        public Member(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        #endregion

        #region Properties

        private Estimation estimation;

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
                return this.estimation;
            }

            set
            {
                if (this.estimation != value)
                {
                    if (value != null && !this.Team.AvailableEstimations.Contains(value))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_EstimationIsNotAvailableInTeam, value.Value), "value");
                    }

                    this.estimation = value;
                    if (this.Team.State == TeamState.EstimationInProgress)
                    {
                        this.Team.OnMemberEstimated(this);
                    }
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Resets the estimation to unselected.
        /// </summary>
        internal void ResetEstimation()
        {
            this.estimation = null;
        }

        #endregion
    }
}
