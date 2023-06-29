using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Represents error or invalid input in an operation in the Planning Poker application.
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
        /// <param name="error">The error code of the error that caused this exception.</param>
        /// <param name="argument">The argument value that was invalid input for the failed operation.</param>
        public PlanningPokerException(string error, string argument)
            : this(GetExceptionMessage(error, argument), error, argument)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="error">The error code of the error that caused this exception.</param>
        /// <param name="argument">The argument value that was invalid input for the failed operation.</param>
        public PlanningPokerException(string message, string error, string argument)
            : base(message)
        {
            Error = error;
            Argument = argument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected PlanningPokerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Error = info.GetString(nameof(Error));
            Argument = info.GetString(nameof(Argument));
        }

        /// <summary>
        /// Gets an error code of the error that caused this exception.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets an argument value that was invalid input for the failed operation.
        /// </summary>
        public string? Argument { get; }

        private static string GetExceptionMessage(string error, string argument)
        {
            string? message = error switch
            {
                ErrorCodes.ScrumTeamNotExist => Resources.Error_ScrumTeamNotExist,
                ErrorCodes.ScrumTeamAlreadyExists => Resources.Error_ScrumTeamAlreadyExists,
                ErrorCodes.MemberAlreadyExists => Resources.Error_MemberAlreadyExists,
                _ => null
            };

            if (message == null)
            {
                return error;
            }

            return string.Format(CultureInfo.InvariantCulture, message, argument);
        }
    }
}
