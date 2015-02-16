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
    /// Status of participant in estimation.
    /// </summary>
    [Serializable]
    [DataContract(Name = "estimationParticipantStatus", Namespace = Namespaces.PlanningPokerData)]
    public class EstimationParticipantStatus
    {
        /// <summary>
        /// Gets or sets the name of the participant.
        /// </summary>
        /// <value>
        /// The name of the member.
        /// </value>
        [DataMember(Name = "memberName")]
        public string MemberName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this participant submitted an estimate already.
        /// </summary>
        /// <value>
        /// <c>True</c> if participant estimated; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "estimated")]
        public bool Estimated { get; set; }
    }
}
