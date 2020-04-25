using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Scrum Team member data for serialization.
    /// </summary>
    public class MemberData
    {
        /// <summary>
        /// Gets or sets the member's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets type of Scrum Team member.
        /// </summary>
        public MemberType MemberType { get; set; }

        /// <summary>
        /// Gets or sets the collection messages sent to the member.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
        public IList<MessageData> Messages { get; set; }

        /// <summary>
        /// Gets or sets ID of last message received by client.
        /// </summary>
        public long LastMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last time, the member checked for new messages.
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is connected or not.
        /// </summary>
        public bool IsDormant { get; set; }

        /// <summary>
        /// Gets or sets the estimation, the member is picking in planning poker.
        /// </summary>
        public Estimation Estimation { get; set; }
    }
}
