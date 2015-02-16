// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Service providing operations for planning poker web clients.
    /// </summary>
    [ServiceBehavior(Namespace = Namespaces.PlanningPokerService, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PlanningPokerService : IPlanningPokerService
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerService"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker controller.</param>
        public PlanningPokerService(D.IPlanningPoker planningPoker)
        {
            if (planningPoker == null)
            {
                throw new ArgumentNullException("planningPoker");
            }

            this.PlanningPoker = planningPoker;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the planning poker controller.
        /// </summary>
        public D.IPlanningPoker PlanningPoker { get; private set; }

        #endregion

        #region IPlanningPokerService

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
            ValidateTeamName(teamName);
            ValidateMemberName(scrumMasterName, "scrumMasterName");

            try
            {
                using (var teamLock = this.PlanningPoker.CreateScrumTeam(teamName, scrumMasterName))
                {
                    teamLock.Lock();
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                throw new FaultException(ex.Message);
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
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, "memberName");

            try
            {
                using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    var team = teamLock.Team;
                    team.Join(memberName, asObserver);
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                throw new FaultException(ex.Message);
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
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, "memberName");

            try
            {
                using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    var team = teamLock.Team;
                    var observer = team.FindMemberOrObserver(memberName);
                    if (observer == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.Error_MemberNotFound, memberName), "memberName");
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
                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        public void DisconnectTeam(string teamName, string memberName)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, "memberName");

            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
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
            ValidateTeamName(teamName);

            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
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
            ValidateTeamName(teamName);

            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
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
        public void SubmitEstimation(string teamName, string memberName, double estimation)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, "memberName");

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

            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
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
        /// <param name="callback">The callback delegate to call, when the member receives new message.</param>
        /// <param name="asyncState">State object of asynchronous operation.</param>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> object representing asynchronous operation.
        /// </returns>
        public IAsyncResult BeginGetMessages(string teamName, string memberName, long lastMessageId, AsyncCallback callback, object asyncState)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, "memberName");

            var getMessagesTask = new GetMessagesTask(this.PlanningPoker)
            {
                TeamName = teamName,
                MemberName = memberName,
                LastMessageId = lastMessageId,
                Callback = callback,
                AsyncState = asyncState
            };
            getMessagesTask.Start();

            return getMessagesTask.ProcessMessagesTask;
        }

        /// <summary>
        /// Ends the asynchronous operation of getting messages for specified team member.
        /// </summary>
        /// <param name="ar">The <see cref="T:System.IAsyncResult"/> object.</param>
        /// <returns>
        /// Collection of new messages sent to the team member.
        /// </returns>
        public IList<Message> EndGetMessages(IAsyncResult ar)
        {
            if (ar == null)
            {
                throw new ArgumentNullException("ar");
            }

            var task = ar as Task<List<Message>>;
            if (task == null)
            {
                throw new ArgumentException(Properties.Resources.Error_EndGetMessagesAsyncResult, "ar");
            }

            return task.Result;
        }

        #endregion

        #region Private methods

        private static void ValidateTeamName(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException("teamName");
            }

            if (teamName.Length > 50)
            {
                throw new ArgumentException(Properties.Resources.Error_TeamNameTooLong, "teamName");
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
                throw new ArgumentException(Properties.Resources.Error_TeamNameTooLong, paramName);
            }
        }

        #endregion

        #region Inner types

        /// <summary>
        /// Asynchronous task of receiving messages by a team member.
        /// </summary>
        private class GetMessagesTask
        {
            #region Fields

            private TaskCompletionSource<List<Message>> taskCompletionSource;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="GetMessagesTask"/> class.
            /// </summary>
            /// <param name="planningPoker">The planning poker controller.</param>
            public GetMessagesTask(D.IPlanningPoker planningPoker)
            {
                this.PlanningPoker = planningPoker;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the planning poker controller.
            /// </summary>
            /// <value>
            /// The planning poker controller.
            /// </value>
            public D.IPlanningPoker PlanningPoker { get; private set; }

            /// <summary>
            /// Gets or sets the name of the Scrum team.
            /// </summary>
            /// <value>
            /// The name of the Scrum team.
            /// </value>
            public string TeamName { get; set; }

            /// <summary>
            /// Gets or sets the name of the member to receive message of.
            /// </summary>
            /// <value>
            /// The name of the team member.
            /// </value>
            public string MemberName { get; set; }

            /// <summary>
            /// Gets or sets ID of last message received by the team member.
            /// </summary>
            /// <value>
            /// The last message ID.
            /// </value>
            public long LastMessageId { get; set; }

            /// <summary>
            /// Gets or sets the callback delegate to call when a new message is received.
            /// </summary>
            /// <value>
            /// The callback delegate.
            /// </value>
            public AsyncCallback Callback { get; set; }

            /// <summary>
            /// Gets or sets the state object of asynchronous operation.
            /// </summary>
            /// <value>
            /// The state object.
            /// </value>
            public object AsyncState { get; set; }

            /// <summary>
            /// Gets the asynchronous operation of receiving messages for the team member.
            /// </summary>
            public Task<List<Message>> ProcessMessagesTask
            {
                get
                {
                    return this.taskCompletionSource != null ? this.taskCompletionSource.Task : null;
                }
            }

            #endregion

            #region Public methods

            /// <summary>
            /// Starts the asynchronous operation of receiving new messages.
            /// </summary>
            public void Start()
            {
                this.taskCompletionSource = new TaskCompletionSource<List<Message>>(this.AsyncState);
                Task.Factory.StartNew(this.SetProcessMessagesHandler);
            }

            #endregion

            #region Private methods

            private void ReturnResult(List<Message> result)
            {
                Task.Factory.StartNew(() =>
                {
                    this.taskCompletionSource.SetResult(result);
                    if (this.Callback != null)
                    {
                        this.Callback(this.ProcessMessagesTask);
                    }
                });
            }

            private void ThrowException(Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    this.taskCompletionSource.SetException(ex);
                    if (this.Callback != null)
                    {
                        this.Callback(this.ProcessMessagesTask);
                    }
                });
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions must be set as asynchronous task result.")]
            private void ProcessMessages(bool hasMessages, D.Observer member)
            {
                try
                {
                    List<Message> result;
                    if (hasMessages)
                    {
                        result = member.Messages.Select(m => ServiceEntityMapper.Map<D.Message, Message>(m)).ToList();
                    }
                    else
                    {
                        result = new List<Message>();
                    }

                    this.ReturnResult(result);
                }
                catch (Exception ex)
                {
                    this.ThrowException(ex);
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions must be set as asynchronous task result.")]
            private void SetProcessMessagesHandler()
            {
                try
                {
                    using (var teamLock = this.PlanningPoker.GetScrumTeam(this.TeamName))
                    {
                        teamLock.Lock();
                        var team = teamLock.Team;
                        var member = team.FindMemberOrObserver(this.MemberName);

                        // Removes old messages, which the member has already read, from the member's message queue.
                        while (member.HasMessage && member.Messages.First().Id <= this.LastMessageId)
                        {
                            member.PopMessage();
                        }

                        // Updates last activity on member to record time, when member checked for new messages.
                        // also notifies to save the team into repository
                        member.UpdateActivity();

                        this.PlanningPoker.GetMessagesAsync(member, this.ProcessMessages);
                    }
                }
                catch (Exception ex)
                {
                    this.ThrowException(ex);
                }
            }

            #endregion
        }

        #endregion
    }
}
