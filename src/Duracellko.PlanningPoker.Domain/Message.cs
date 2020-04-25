namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Generic message that can be sent to Scrum team members or observers.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        public Message(MessageType type)
        {
            MessageType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="id">The message ID.</param>
        public Message(MessageType type, long id)
        {
            MessageType = type;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="messageData">Message serialization data.</param>
        internal Message(Serialization.MessageData messageData)
        {
            MessageType = messageData.MessageType;
            Id = messageData.Id;
        }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public MessageType MessageType { get; private set; }

        /// <summary>
        /// Gets the message ID unique to member, so that member can track which messages he/she already got.
        /// </summary>
        /// <value>The message ID.</value>
        public long Id { get; internal set; }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        protected internal virtual Serialization.MessageData GetData()
        {
            return new Serialization.MessageData
            {
                MessageType = MessageType,
                Id = Id
            };
        }
    }
}
