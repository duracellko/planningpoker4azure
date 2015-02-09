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
    /// Message of event in specific Scrum team related to a member. For example when a member connects or disconnects.
    /// </summary>
    public class ScrumTeamMemberMessage : ScrumTeamMessage
    {
        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberMessage"/> class.
        /// </summary>
        public ScrumTeamMemberMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberMessage"/> class.
        /// </summary>
        /// <param name="teamName">The name of team, this message is related to.</param>
        /// <param name="messageType">The type of message.</param>
        public ScrumTeamMemberMessage(string teamName, MessageType messageType)
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
        /// Gets or sets a type of member.
        /// </summary>
        /// <value>The member type.</value>
        public string MemberType { get; set; }

        #endregion
    }
}
