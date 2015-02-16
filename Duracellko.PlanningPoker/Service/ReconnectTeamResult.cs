// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Result data of reconnect operation.
    /// </summary>
    [Serializable]
    [DataContract(Name = "reconnectTeamResult", Namespace = Namespaces.PlanningPokerData)]
    public class ReconnectTeamResult
    {
        /// <summary>
        /// Gets or sets the Scrum team.
        /// </summary>
        /// <value>
        /// The Scrum team.
        /// </value>
        [DataMember(Name = "scrumTeam")]
        public ScrumTeam ScrumTeam { get; set; }

        /// <summary>
        /// Gets or sets the last message ID for the member.
        /// </summary>
        /// <value>
        /// The last message ID.
        /// </value>
        [DataMember(Name = "lastMessageId")]
        public long LastMessageId { get; set; }
    }
}
