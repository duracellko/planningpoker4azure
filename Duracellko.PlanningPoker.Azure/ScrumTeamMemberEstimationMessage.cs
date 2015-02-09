// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Message of event in specific Scrum team, which notifies that a member placed estimation.
    /// </summary>
    public class ScrumTeamMemberEstimationMessage : ScrumTeamMessage
    {
        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimationMessage"/> class.
        /// </summary>
        public ScrumTeamMemberEstimationMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimationMessage"/> class.
        /// </summary>
        /// <param name="teamName">The name of team, this message is related to.</param>
        /// <param name="messageType">The type of message.</param>
        public ScrumTeamMemberEstimationMessage(string teamName, MessageType messageType)
            : base(teamName, messageType)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a name of member, which this message is related to.
        /// </summary>
        /// <value>The member name.</value>
        public string MemberName { get; set; }

        /// <summary>
        /// Gets or sets a member's estimation.
        /// </summary>
        /// <value>The member's estimation.</value>
        public double? Estimation { get; set; }

        #endregion
    }
}
