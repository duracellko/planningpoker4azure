using System;
using System.Runtime.Serialization;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Represents error returned by Planning Poker service.
    /// </summary>
    public class PlanningPokerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        public PlanningPokerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PlanningPokerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PlanningPokerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected PlanningPokerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
