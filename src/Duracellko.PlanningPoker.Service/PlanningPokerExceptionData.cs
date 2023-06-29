namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Information about error that occured in the Planning Poker application.
    /// </summary>
    public class PlanningPokerExceptionData
    {
        /// <summary>
        /// Gets or sets an error code of the error that caused this exception.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets a message that describes the error.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets an argument value that was invalid input for the failed operation.
        /// </summary>
        public string? Argument { get; set; }
    }
}
