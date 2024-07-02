using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Objects provides difference between client and server time.
/// </summary>
public interface IServiceTimeProvider
{
    /// <summary>
    /// Gets the difference between client and server time.
    /// </summary>
    /// <remarks>
    /// This value is used to convert time in service responses to client time.
    /// This solves problems, when client time is not correctly set.
    /// </remarks>
    TimeSpan ServiceTimeOffset { get; }

    /// <summary>
    /// Obtains server time and updates <see cref="ServiceTimeOffset"/> value.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Asynchronous operation.</returns>
    /// <remarks>
    /// The value is updated only, when it is older than 5 minutes.
    /// </remarks>
    Task UpdateServiceTimeOffset(CancellationToken cancellationToken);
}
