using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Defines methods to manage Scrum teams.
    /// </summary>
    public interface IPlanningPoker
    {
        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        IEnumerable<string> ScrumTeamNames { get; }

        /// <summary>
        /// Creates new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>The new Scrum team.</returns>
        IScrumTeamLock CreateScrumTeam(string teamName, string scrumMasterName);

        /// <summary>
        /// Adds existing Scrum team to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team to add.</param>
        /// <returns>The joined Scrum team.</returns>
        IScrumTeamLock AttachScrumTeam(ScrumTeam team);

        /// <summary>
        /// Gets existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>The Scrum team.</returns>
        IScrumTeamLock GetScrumTeam(string teamName);

        /// <summary>
        /// Gets messages for specified observer asynchronously. Messages are returned, when the observer receives any,
        /// or empty collection is returned after configured timeout.
        /// </summary>
        /// <param name="observer">The observer to return received messages for.</param>
        /// <param name="cancellationToken">Cancellation token to cancel receiving of messages.</param>
        /// <returns>Asynchronous task that is finished, when observer receives a message or after configured timeout.</returns>
        Task<IEnumerable<Message>> GetMessagesAsync(Observer observer, CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for configured period of time.
        /// </summary>
        void DisconnectInactiveObservers();

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        void DisconnectInactiveObservers(TimeSpan inactivityTime);
    }
}
