using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Objects provides operations of Planning Poker service.
    /// </summary>
    public interface IPlanningPokerClient
    {
        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        Task<ScrumTeam> CreateTeam(string teamName, string scrumMasterName, CancellationToken cancellationToken);

        /// <summary>
        /// Connects member or observer with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member or observer.</param>
        /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer joined to.
        /// </returns>
        Task<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken);

        /// <summary>
        /// Reconnects member with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer reconnected to.
        /// </returns>
        /// <remarks>
        /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
        /// </remarks>
        Task<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName, CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        Task DisconnectTeam(string teamName, string memberName, CancellationToken cancellationToken);

        /// <summary>
        /// Signal from Scrum master to starts the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        Task StartEstimation(string teamName, CancellationToken cancellationToken);

        /// <summary>
        /// Signal from Scrum master to cancels the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        Task CancelEstimation(string teamName, CancellationToken cancellationToken);

        /// <summary>
        /// Submits the estimation for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimation">The estimation the member is submitting.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        Task SubmitEstimation(string teamName, string memberName, double? estimation, CancellationToken cancellationToken);

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// List of messages.
        /// </returns>
        Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId, CancellationToken cancellationToken);
    }
}
