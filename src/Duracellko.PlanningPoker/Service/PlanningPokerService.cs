using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Service providing operations for planning poker web clients.
    /// </summary>
    [Route("api/PlanningPokerService")]
    [Controller]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class PlanningPokerService : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerService"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker controller.</param>
        public PlanningPokerService(D.IPlanningPoker planningPoker)
        {
            PlanningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
        }

        /// <summary>
        /// Gets the planning poker controller.
        /// </summary>
        public D.IPlanningPoker PlanningPoker { get; private set; }

        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        [HttpGet("CreateTeam")]
        public ActionResult<ScrumTeam> CreateTeam(string teamName, string scrumMasterName)
        {
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
                return BadRequest(ex.Message);
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
        [HttpGet("JoinTeam")]
        public ActionResult<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver)
        {
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
                return BadRequest(ex.Message);
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
        [HttpGet("ReconnectTeam")]
        public ActionResult<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName)
        {
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
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        [HttpGet("DisconnectTeam")]
        public void DisconnectTeam(string teamName, string memberName)
        {
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
        [HttpGet("StartEstimation")]
        public void StartEstimation(string teamName)
        {
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
        [HttpGet("CancelEstimation")]
        public void CancelEstimation(string teamName)
        {
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
        [HttpGet("SubmitEstimation")]
        public void SubmitEstimation(string teamName, string memberName, double estimation)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            double? domainEstimation;
            if (estimation == -1111111.0)
            {
                domainEstimation = null;
            }
            else if (estimation == Estimation.PositiveInfinity)
            {
                domainEstimation = double.PositiveInfinity;
            }
            else
            {
                domainEstimation = estimation;
            }

            using (var teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                var member = team.FindMemberOrObserver(memberName) as D.Member;
                if (member != null)
                {
                    member.Estimation = new D.Estimation(domainEstimation);
                }
            }
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// Collection of received messages or empty collection, when no message was received in configured time.
        /// </returns>
        [HttpGet("GetMessages")]
        public async Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId, CancellationToken cancellationToken)
        {
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

                receiveMessagesTask = PlanningPoker.GetMessagesAsync(member, cancellationToken);
            }

            var messages = await receiveMessagesTask;
            return messages.Select(ServiceEntityMapper.FilterMessage)
                .Select(ServiceEntityMapper.Map<D.Message, Message>).ToList();
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
    }
}
