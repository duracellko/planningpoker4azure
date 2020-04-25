using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// SignalR hub providing operations for planning poker web clients.
    /// </summary>
    public class PlanningPokerHub : Hub<IPlanningPokerClient>
    {
        private readonly IHubContext<PlanningPokerHub, IPlanningPokerClient> _clientContext;
        private readonly ILogger<PlanningPokerHub> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerHub"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker controller.</param>
        /// <param name="clientContext">Interface to send messages to client.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public PlanningPokerHub(D.IPlanningPoker planningPoker, IHubContext<PlanningPokerHub, IPlanningPokerClient> clientContext, ILogger<PlanningPokerHub> logger)
        {
            PlanningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
            _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
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
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        public ScrumTeam CreateTeam(string teamName, string scrumMasterName)
        {
            _logger.LogInformation("{action}(\"{teamName}\", \"{scrumMasterName}\")", nameof(CreateTeam), teamName, scrumMasterName);
            ValidateTeamName(teamName);
            ValidateMemberName(scrumMasterName, nameof(scrumMasterName));

            try
            {
                using (var teamLock = PlanningPoker.CreateScrumTeam(teamName, scrumMasterName))
                {
                    teamLock.Lock();
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
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
        public ScrumTeam JoinTeam(string teamName, string memberName, bool asObserver)
        {
            _logger.LogInformation("{action}(\"{teamName}\", \"{memberName}\", {asObserver})", nameof(JoinTeam), teamName, memberName, asObserver);
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            try
            {
                using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    var team = teamLock.Team;
                    team.Join(memberName, asObserver);
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
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
            _logger.LogInformation("{action}(\"{teamName}\", \"{memberName}\")", nameof(ReconnectTeam), teamName, memberName);
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            try
            {
                using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    var team = teamLock.Team;
                    var observer = team.FindMemberOrObserver(memberName);
                    if (observer == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberNotFound, memberName), nameof(memberName));
                    }

                    Estimation selectedEstimation = null;
                    if (team.State == D.TeamState.EstimationInProgress)
                    {
                        var member = observer as D.Member;
                        if (member != null)
                        {
                            selectedEstimation = ServiceEntityMapper.Map<D.Estimation, Estimation>(member.Estimation);
                        }
                    }

                    var lastMessageId = observer.ClearMessages();
                    observer.UpdateActivity();

                    var teamResult = ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                    return new ReconnectTeamResult()
                    {
                        ScrumTeam = teamResult,
                        LastMessageId = lastMessageId,
                        SelectedEstimation = selectedEstimation
                    };
                }
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
            _logger.LogInformation("{action}(\"{teamName}\", \"{memberName}\")", nameof(DisconnectTeam), teamName, memberName);
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                team.Disconnect(memberName);
            }
        }

        /// <summary>
        /// Signal from Scrum master to starts the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        public void StartEstimation(string teamName)
        {
            _logger.LogInformation("{action}(\"{teamName}\")", nameof(StartEstimation), teamName);
            ValidateTeamName(teamName);

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                team.ScrumMaster.StartEstimation();
            }
        }

        /// <summary>
        /// Signal from Scrum master to cancels the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        public void CancelEstimation(string teamName)
        {
            _logger.LogInformation("{action}(\"{teamName}\")", nameof(CancelEstimation), teamName);
            ValidateTeamName(teamName);

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                team.ScrumMaster.CancelEstimation();
            }
        }

        /// <summary>
        /// Submits the estimation for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimation">The estimation the member is submitting.</param>
        public void SubmitEstimation(string teamName, string memberName, double? estimation)
        {
            _logger.LogInformation("{action}(\"{teamName}\", \"{memberName}\", {estimation})", nameof(SubmitEstimation), teamName, memberName, estimation);
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            if (estimation == Estimation.PositiveInfinity)
            {
                estimation = double.PositiveInfinity;
            }

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                var member = team.FindMemberOrObserver(memberName) as D.Member;
                if (member != null)
                {
                    member.Estimation = new D.Estimation(estimation);
                }
            }
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        public void GetMessages(string teamName, string memberName, long lastMessageId)
        {
            _logger.LogInformation("{action}(\"{teamName}\", \"{memberName}\", {lastMessageId})", nameof(GetMessages), teamName, memberName, lastMessageId);
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            Task<IEnumerable<D.Message>> receiveMessagesTask;

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                var member = team.FindMemberOrObserver(memberName);

                // Removes old messages, which the member has already read, from the member's message queue.
                while (member.HasMessage && member.Messages.First().Id <= lastMessageId)
                {
                    member.PopMessage();
                }

                // Updates last activity on member to record time, when member checked for new messages.
                // also notifies to save the team into repository
                member.UpdateActivity();

                receiveMessagesTask = PlanningPoker.GetMessagesAsync(member, Context.ConnectionAborted);
            }

            OnMessageReceived(receiveMessagesTask, Context.ConnectionId);
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
                throw new ArgumentException(Resources.Error_TeamNameTooLong, paramName);
            }
        }

        private async void OnMessageReceived(Task<IEnumerable<D.Message>> receiveMessagesTask, string connectionId)
        {
            try
            {
                var messages = await receiveMessagesTask;
                var clientMessages = messages.Select(ServiceEntityMapper.FilterMessage)
                    .Select(ServiceEntityMapper.Map<D.Message, Message>).ToList();

                _logger.LogDebug("Notify messages received (connectionId: {connectionId})", connectionId);
                var client = _clientContext.Clients.Client(connectionId);
                await client.Notify(clientMessages);
            }
            catch (OperationCanceledException)
            {
                // Operation is canceled, because client has disconnected.
            }
        }
    }
}
