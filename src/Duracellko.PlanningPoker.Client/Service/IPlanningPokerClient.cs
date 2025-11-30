using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Service;

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
    /// <param name="deck">Selected deck of estimation cards to use in the team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Created Scrum team.
    /// </returns>
    Task<TeamResult> CreateTeam(string teamName, string scrumMasterName, Deck deck, CancellationToken cancellationToken);

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
    Task<TeamResult> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken);

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
    /// Signal from Scrum master to start the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task StartEstimation(string teamName, CancellationToken cancellationToken);

    /// <summary>
    /// Signal from Scrum master to cancel the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task CancelEstimation(string teamName, CancellationToken cancellationToken);

    /// <summary>
    /// Signal from Scrum master to close the estimation by assigning nil vote to unvoted members.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task CloseEstimation(string teamName, CancellationToken cancellationToken);

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
    /// Changes deck of estimation cards, if estimation is not in progress.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="deck">New deck of estimation cards to use in the team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task ChangeDeck(string teamName, Deck deck, CancellationToken cancellationToken);

    /// <summary>
    /// Starts countdown timer for team with specified duration.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="duration">Duration of countdown timer.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task StartTimer(string teamName, string memberName, TimeSpan duration, CancellationToken cancellationToken);

    /// <summary>
    /// Stops active countdown timer.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    Task CancelTimer(string teamName, string memberName, CancellationToken cancellationToken);

    /// <summary>
    /// Begins to get messages of specified member asynchronously.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="sessionId">The session ID for receiving messages.</param>
    /// <param name="lastMessageId">ID of last message the member received.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// List of messages.
    /// </returns>
    Task<IList<Message>> GetMessages(string teamName, string memberName, Guid sessionId, long lastMessageId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets information about current time of service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Current time of service in UTC time zone.</returns>
    Task<TimeResult> GetCurrentTime(CancellationToken cancellationToken);
}
