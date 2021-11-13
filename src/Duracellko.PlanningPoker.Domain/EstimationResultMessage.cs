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
        /// <param name="estimationResult">The estimation result associated to the message.</param>
        public EstimationResultMessage(MessageType type, EstimationResult estimationResult)
            : base(type)
        {
            EstimationResult = estimationResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationResultMessage"/> class.
        /// </summary>
        /// <param name="messageData">Message serialization data.</param>
        /// <param name="estimationResult">The estimation result associated to the message.</param>
        internal EstimationResultMessage(Serialization.MessageData messageData, EstimationResult estimationResult)
            : base(messageData)
        {
            EstimationResult = estimationResult;
        }

        /// <summary>
        /// Gets the estimation result associated to the message.
        /// </summary>
        /// <value>
        /// The estimation result.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Message is sent to client and forgotten. Client can modify it as it wants.")]
        public EstimationResult EstimationResult { get; }

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
