using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// SignalR hub providing operations for planning poker web clients.
/// </summary>
public class PlanningPokerHub : Hub<IPlanningPokerClient>
{
    private static readonly CompositeFormat _errorMemberNotFound = CompositeFormat.Parse(Resources.Error_MemberNotFound);

    private readonly IHubContext<PlanningPokerHub, IPlanningPokerClient> _clientContext;
    private readonly D.DateTimeProvider _dateTimeProvider;
    private readonly D.DeckProvider _deckProvider;
    private readonly ILogger<PlanningPokerHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerHub"/> class.
    /// </summary>
    /// <param name="planningPoker">The planning poker controller.</param>
    /// <param name="clientContext">Interface to send messages to client.</param>
    /// <param name="dateTimeProvider">The date time provider to provide current time.</param>
    /// <param name="deckProvider">The provider to get estimation cards deck.</param>
    /// <param name="logger">Logger instance to log events.</param>
    public PlanningPokerHub(
        D.IPlanningPoker planningPoker,
        IHubContext<PlanningPokerHub, IPlanningPokerClient> clientContext,
        D.DateTimeProvider dateTimeProvider,
        D.DeckProvider deckProvider,
        ILogger<PlanningPokerHub> logger)
    {
        PlanningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
        _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _deckProvider = deckProvider ?? throw new ArgumentNullException(nameof(deckProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the planning poker controller.
    /// </summary>
    public D.IPlanningPoker PlanningPoker { get; }

    /// <summary>
    /// Creates new Scrum team with specified team name and Scrum master name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="scrumMasterName">Name of the Scrum master.</param>
    /// <param name="deck">Selected deck of estimation cards to use in the team.</param>
    /// <returns>
    /// Created Scrum team.
    /// </returns>
    public TeamResult CreateTeam(string teamName, string scrumMasterName, Deck deck)
    {
        _logger.CreateTeam(teamName, scrumMasterName, deck);
        ValidateTeamName(teamName);
        ValidateMemberName(scrumMasterName, nameof(scrumMasterName));

        try
        {
            var domainDeck = ServiceEntityMapper.Map(deck);

            using var teamLock = PlanningPoker.CreateScrumTeam(teamName, scrumMasterName, domainDeck);
            teamLock.Lock();
            var resultTeam = ServiceEntityMapper.Map(teamLock.Team);
            return new TeamResult
            {
                ScrumTeam = resultTeam,
                SessionId = teamLock.Team.ScrumMaster!.SessionId
            };
        }
        catch (D.PlanningPokerException ex)
        {
            throw CreateHubException(ex);
        }
    }

    /// <summary>
    /// Connects member or observer with specified name to the Scrum team with specified name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member or observer.</param>
    /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
    /// <returns>
    /// The Scrum team the member or observer joined to.
    /// </returns>
    public TeamResult JoinTeam(string teamName, string memberName, bool asObserver)
    {
        _logger.JoinTeam(teamName, memberName, asObserver);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        try
        {
            using var teamLock = PlanningPoker.GetScrumTeam(teamName);
            teamLock.Lock();
            var team = teamLock.Team;
            var member = team.Join(memberName, asObserver);

            var resultTeam = ServiceEntityMapper.Map(teamLock.Team);
            return new TeamResult
            {
                ScrumTeam = resultTeam,
                SessionId = member.SessionId
            };
        }
        catch (D.PlanningPokerException ex)
        {
            throw CreateHubException(ex);
        }
    }

    /// <summary>
    /// Reconnects member with specified name to the Scrum team with specified name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <returns>
    /// The Scrum team the member or observer reconnected to.
    /// </returns>
    /// <remarks>
    /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
    /// </remarks>
    public ReconnectTeamResult ReconnectTeam(string teamName, string memberName)
    {
        _logger.ReconnectTeam(teamName, memberName);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        try
        {
            using var teamLock = PlanningPoker.GetScrumTeam(teamName);
            teamLock.Lock();
            var team = teamLock.Team;
            var observer = team.CreateSession(memberName);
            if (observer == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _errorMemberNotFound, memberName), nameof(memberName));
            }

            Estimation? selectedEstimation = null;
            if (team.State == D.TeamState.EstimationInProgress && observer is D.Member member)
            {
                selectedEstimation = ServiceEntityMapper.Map(member.Estimation);
            }

            var lastMessageId = observer.ClearMessages();
            observer.UpdateActivity();

            var resultTeam = ServiceEntityMapper.Map(teamLock.Team);
            return new ReconnectTeamResult()
            {
                ScrumTeam = resultTeam,
                SessionId = observer.SessionId,
                LastMessageId = lastMessageId,
                SelectedEstimation = selectedEstimation
            };
        }
        catch (D.PlanningPokerException ex)
        {
            throw CreateHubException(ex);
        }
        catch (ArgumentException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>
    /// Disconnects member from the Scrum team.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    public void DisconnectTeam(string teamName, string memberName)
    {
        _logger.DisconnectTeam(teamName, memberName);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        team.Disconnect(memberName);
    }

    /// <summary>
    /// Signal from Scrum master to start the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    public void StartEstimation(string teamName)
    {
        _logger.StartEstimation(teamName);
        ValidateTeamName(teamName);

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        team.ScrumMaster?.StartEstimation();
    }

    /// <summary>
    /// Signal from Scrum master to cancel the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    public void CancelEstimation(string teamName)
    {
        _logger.CancelEstimation(teamName);
        ValidateTeamName(teamName);

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        team.ScrumMaster?.CancelEstimation();
    }

    /// <summary>
    /// Submits the estimation for specified team member.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="estimation">The estimation the member is submitting.</param>
    public void SubmitEstimation(string teamName, string memberName, double? estimation)
    {
        _logger.SubmitEstimation(teamName, memberName, estimation);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        if (estimation == Estimation.PositiveInfinity)
        {
            estimation = double.PositiveInfinity;
        }

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        if (team.FindMemberOrObserver(memberName) is D.Member member)
        {
            member.Estimation = new D.Estimation(estimation);
        }
    }

    /// <summary>
    /// Changes deck of estimation cards, if estimation is not in progress.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="deck">New deck of estimation cards to use in the team.</param>
    public void ChangeDeck(string teamName, Deck deck)
    {
        _logger.ChangeDeck(teamName, deck);
        ValidateTeamName(teamName);

        var domainDeck = ServiceEntityMapper.Map(deck);
        var availableEstimations = _deckProvider.GetDeck(domainDeck);

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        team.ChangeAvailableEstimations(availableEstimations);
    }

    /// <summary>
    /// Starts countdown timer for team with specified duration.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="duration">Duration of countdown timer.</param>
    public void StartTimer(string teamName, string memberName, TimeSpan duration)
    {
        _logger.StartTimer(teamName, memberName, duration);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, Resources.Error_InvalidTimerDuraction);
        }

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        var member = team.FindMemberOrObserver(memberName) as D.Member;
        member?.StartTimer(duration);
    }

    /// <summary>
    /// Stops active countdown timer.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    public void CancelTimer(string teamName, string memberName)
    {
        _logger.CancelTimer(teamName, memberName);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        using var teamLock = PlanningPoker.GetScrumTeam(teamName);
        teamLock.Lock();
        var team = teamLock.Team;
        var member = team.FindMemberOrObserver(memberName) as D.Member;
        member?.CancelTimer();
    }

    /// <summary>
    /// Begins to get messages of specified member asynchronously.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="sessionId">The session ID for receiving messages.</param>
    /// <param name="lastMessageId">ID of last message the member received.</param>
    public void GetMessages(string teamName, string memberName, Guid sessionId, long lastMessageId)
    {
        _logger.GetMessages(teamName, memberName, sessionId, lastMessageId);
        ValidateTeamName(teamName);
        ValidateMemberName(memberName, nameof(memberName));

        Task<IEnumerable<D.Message>> receiveMessagesTask;

        using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
        {
            teamLock.Lock();
            var team = teamLock.Team;
            var member = team.FindMemberOrObserver(memberName);

            if (member == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _errorMemberNotFound, memberName), nameof(memberName));
            }

            // Removes old messages, which the member has already read, from the member's message queue.
            try
            {
                member.AcknowledgeMessages(sessionId, lastMessageId);
            }
            catch (ArgumentException ex) when (ex.ParamName == "sessionId")
            {
                throw new HubException(ex.Message);
            }

            // Updates last activity on member to record time, when member checked for new messages.
            // also notifies to save the team into repository
            member.UpdateActivity();

            receiveMessagesTask = PlanningPoker.GetMessagesAsync(member, Context.ConnectionAborted);
        }

        OnMessageReceived(receiveMessagesTask, Context.ConnectionId);
    }

    /// <summary>
    /// Gets information about current time of service.
    /// </summary>
    /// <returns>Current time of service in UTC time zone.</returns>
    [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "GetCurrentTime is SignalR method.")]
    public TimeResult GetCurrentTime()
    {
        return new TimeResult
        {
            CurrentUtcTime = _dateTimeProvider.UtcNow
        };
    }

    private static void ValidateTeamName(string teamName)
    {
        if (string.IsNullOrEmpty(teamName))
        {
            throw new ArgumentNullException(nameof(teamName));
        }

        if (teamName.Length > 50)
        {
            throw new ArgumentException(Resources.Error_TeamNameTooLong, nameof(teamName));
        }
    }

    private static void ValidateMemberName(string memberName, string paramName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            throw new ArgumentNullException(paramName);
        }

        if (memberName.Length > 50)
        {
            throw new ArgumentException(Resources.Error_MemberNameTooLong, paramName);
        }
    }

    private static HubException CreateHubException(D.PlanningPokerException exception)
    {
        var exceptionData = ServiceEntityMapper.Map(exception);
        return new HubException(nameof(D.PlanningPokerException) + ':' + JsonSerializer.Serialize(exceptionData));
    }

    private async void OnMessageReceived(Task<IEnumerable<D.Message>> receiveMessagesTask, string connectionId)
    {
        try
        {
            var messages = await receiveMessagesTask;
            var clientMessages = messages.Select(ServiceEntityMapper.FilterMessage)
                .Select(ServiceEntityMapper.Map).ToList();

            _logger.MessageReceived(connectionId);
            var client = _clientContext.Clients.Client(connectionId);
            await client.Notify(clientMessages);
        }
        catch (OperationCanceledException)
        {
            // Operation is canceled, because client has disconnected.
        }
    }
}
