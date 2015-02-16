// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Status of participant in estimation.
    /// </summary>
    public class EstimationParticipantStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationParticipantStatus"/> class.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimated">If set to <c>true</c> then the member already estimated.</param>
        public EstimationParticipantStatus(string memberName, bool estimated)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException("memberName");
            }

            this.MemberName = memberName;
            this.Estimated = estimated;
        }

        /// <summary>
        /// Gets the name of the participant.
        /// </summary>
        /// <value>
        /// The name of the member.
        /// </value>
        public string MemberName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this participant submitted an estimate already.
        /// </summary>
        /// <value>
        /// <c>True</c> if participant estimated; otherwise, <c>false</c>.
        /// </value>
        public bool Estimated { get; private set; }
    }
}
