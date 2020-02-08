using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Message sent to all members and observers after all members picked an estimation. The message contains <see cref="EstimationResult"/>.
    /// </summary>
    public class EstimationResultMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationResultMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        public EstimationResultMessage(MessageType type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationResultMessage"/> class.
        /// </summary>
        /// <param name="messageData">Message serialization data.</param>
        internal EstimationResultMessage(Serialization.MessageData messageData)
            : base(messageData)
        {
        }

        /// <summary>
        /// Gets or sets the estimation result associated to the message.
        /// </summary>
        /// <value>
        /// The estimation result.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Message is sent to client and forgotten. Client can modify it as it wants.")]
        public EstimationResult EstimationResult { get; set; }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        protected internal override Serialization.MessageData GetData()
        {
            var result = base.GetData();
            result.EstimationResult = EstimationResult.GetData();
            return result;
        }
    }
}
