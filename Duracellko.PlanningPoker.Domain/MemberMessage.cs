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
    /// Message sent to other members of Scrum team, when a new member joins the team or someone disconnects the planning poker.
    /// </summary>
    [Serializable]
    public class MemberMessage : Message
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        public MemberMessage(MessageType type)
            : base(type)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Scrum team member, who joined or disconnected.
        /// </summary>
        /// <value>
        /// The team member.
        /// </value>
        public Observer Member { get; set; }

        #endregion
    }
}
