﻿namespace Duracellko.PlanningPoker.Domain
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
        public MemberMessage(MessageType type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMessage"/> class.
        /// </summary>
        /// <param name="messageData">Message serialization data.</param>
        internal MemberMessage(Serialization.MessageData messageData)
            : base(messageData)
        {
        }

        /// <summary>
        /// Gets or sets the Scrum team member, who joined or disconnected.
        /// </summary>
        /// <value>
        /// The team member.
        /// </value>
        public Observer Member { get; set; }

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
