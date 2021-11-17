using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Message data for serialization.
    /// </summary>
    public class MessageData
    {
        /// <summary>
        /// Gets or sets the message ID unique to member.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets member name, when type of message is Member Message.
        /// </summary>
        public string? MemberName { get; set; }

        /// <summary>
        /// Gets or sets the estimation result, when type of message is Estimation Result Message.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
        public IDictionary<string, Estimation?>? EstimationResult { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC time zone that specifies, when countdown ends. This is specified for Timer Started message.
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
