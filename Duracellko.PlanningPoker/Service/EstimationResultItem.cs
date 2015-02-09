// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Item of estimation result. It specifies what member picked what estimation.
    /// </summary>
    [Serializable]
    [DataContract(Name = "estimationResultItem", Namespace = Namespaces.PlanningPokerData)]
    public class EstimationResultItem
    {
        /// <summary>
        /// Gets or sets the member, who picked an estimation.
        /// </summary>
        /// <value>
        /// The Scrum team member.
        /// </value>
        [DataMember(Name = "member")]
        public TeamMember Member { get; set; }

        /// <summary>
        /// Gets or sets the estimation picked by the member.
        /// </summary>
        /// <value>
        /// The picked estimation.
        /// </value>
        [DataMember(Name = "estimation")]
        public Estimation Estimation { get; set; }
    }
}
