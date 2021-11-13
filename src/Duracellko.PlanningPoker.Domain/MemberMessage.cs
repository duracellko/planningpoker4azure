namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Message sent to other members of Scrum team, when a new member joins the team or someone disconnects the planning poker.
    /// </summary>
    public class MemberMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="member">The member, who joined or disconnected.</param>
        public MemberMessage(MessageType type, Observer member)
            : base(type)
        {
            Member = member;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMessage"/> class.
        /// </summary>
        /// <param name="messageData">Message serialization data.</param>
        /// <param name="member">The member, who joined or disconnected.</param>
        internal MemberMessage(Serialization.MessageData messageData, Observer member)
            : base(messageData)
        {
            Member = member;
        }

        /// <summary>
        /// Gets the Scrum team member, who joined or disconnected.
        /// </summary>
        /// <value>
        /// The team member.
        /// </value>
        public Observer Member { get; }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        protected internal override Serialization.MessageData GetData()
        {
            var result = base.GetData();
            result.MemberName = Member.Name;
            return result;
        }
    }
}
