using System;

namespace Duracellko.PlanningPoker.Client.Service;

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
    public PlanningPokerException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public PlanningPokerException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="error">The error code of the error that caused this exception.</param>
    /// <param name="argument">The argument value that was invalid input for the failed operation.</param>
    public PlanningPokerException(string? message, string? error, string? argument)
        : this(message, error, argument, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="error">The error code of the error that caused this exception.</param>
    /// <param name="argument">The argument value that was invalid input for the failed operation.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public PlanningPokerException(string? message, string? error, string? argument, Exception? innerException)
        : base(message, innerException)
    {
        Error = error;
        Argument = argument;
    }

    /// <summary>
    /// Gets an error code of the error that caused this exception.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets an argument value that was invalid input for the failed operation.
    /// </summary>
    public string? Argument { get; }
}
