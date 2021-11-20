using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Object provides zero time difference between client and server time.
    /// Application runs on server side, and so the time in the application and server is the same.
    /// </summary>
    public class ServerSideServiceTimeProvider : IServiceTimeProvider
    {
        /// <summary>
        /// Gets the difference between client and server time.
        /// </summary>
        /// <remarks>
        /// The time difference is always zero, because application runs on server side.
        /// </remarks>
        public TimeSpan ServiceTimeOffset => TimeSpan.Zero;

        /// <summary>
        /// Obtains server time and updates <see cref="ServiceTimeOffset"/> value.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>Asynchronous operation.</returns>
        /// <remarks>
        /// This method does not do anything, because application runs on server side.
        /// </remarks>
        public Task UpdateServiceTimeOffset(CancellationToken cancellationToken)
        {
            // No update is needed because application runs directly on server side.
            return Task.CompletedTask;
        }
    }
}
