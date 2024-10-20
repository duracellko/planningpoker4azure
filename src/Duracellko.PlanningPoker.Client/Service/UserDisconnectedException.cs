using System;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Error that the user is disconnected from the team and the service does not accept any requests from the particular user.
/// </summary>
public class UserDisconnectedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDisconnectedException"/> class.
    /// </summary>
    public UserDisconnectedException()
        : this(default(string?))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDisconnectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UserDisconnectedException(string? message)
        : base(message ?? Resources.Error_UserDisconnected)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDisconnectedException"/> class.
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public UserDisconnectedException(Exception? innerException)
        : this(null, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDisconnectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public UserDisconnectedException(string? message, Exception? innerException)
        : base(message ?? Resources.Error_UserDisconnected, innerException)
    {
    }
}
