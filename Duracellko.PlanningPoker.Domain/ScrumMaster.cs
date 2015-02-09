// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum master can additionally to member start and cancel estimation planning poker.
    /// </summary>
    [Serializable]
    public class ScrumMaster : Member
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumMaster"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the the master is joining.</param>
        /// <param name="name">The Scrum master name.</param>
        public ScrumMaster(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts new estimation.
        /// </summary>
        public void StartEstimation()
        {
            if (this.Team.State == TeamState.EstimationInProgress)
            {
                throw new InvalidOperationException(Properties.Resources.Error_EstimationIsInProgress);
            }

            this.Team.StartEstimation();
        }

        /// <summary>
        /// Cancels current estimation.
        /// </summary>
        public void CancelEstimation()
        {
            if (this.Team.State == TeamState.EstimationInProgress)
            {
                this.Team.CancelEstimation();
            }
        }

        #endregion
    }
}
