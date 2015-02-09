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
    /// Message of event in specific Scrum team.
    /// </summary>
    public class ScrumTeamMessage
    {
        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMessage"/> class.
        /// </summary>
        public ScrumTeamMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMessage"/> class.
        /// </summary>
        /// <param name="teamName">The name of team, this message is related to.</param>
        /// <param name="messageType">The type of message.</param>
        public ScrumTeamMessage(string teamName, MessageType messageType)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException("teamName");
            }

            this.TeamName = teamName;
            this.MessageType = messageType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a name of team, which this message is related to.
        /// </summary>
        /// <value>The Scrum team name.</value>
        public string TeamName { get; set; }

        /// <summary>
        /// Gets or sets a type of message sent by a Scrum team.
        /// </summary>
        /// <value>The message type.</value>
        public MessageType MessageType { get; set; }

        #endregion
    }
}
