// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Instance of Planning Poker application in Windows Azure. Synchronizes the planning poker teams with other nodes.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class PlanningPokerAzureNode : IDisposable
    {
        #region Fields

        private IDisposable sendNodeMessageSubscription;
        private IDisposable serviceBusScrumTeamMessageSubscription;
        private IDisposable serviceBusTeamCreatedMessageSubscription;
        private IDisposable serviceBusRequestTeamListMessageSubscription;
        private IDisposable serviceBusRequestTeamsMessageSubscription;

        private volatile string processingScrumTeamName;
        private List<string> teamsToInitialize;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerAzureNode"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker teams controller instance.</param>
        /// <param name="serviceBus">The service bus used to send messages between nodes.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        public PlanningPokerAzureNode(IAzurePlanningPoker planningPoker, IServiceBus serviceBus, IAzurePlanningPokerConfiguration configuration)
        {
            if (planningPoker == null)
            {
                throw new ArgumentNullException("planningPoker");
            }

            if (serviceBus == null)
            {
                throw new ArgumentNullException("serviceBus");
            }

            this.PlanningPoker = planningPoker;
            this.ServiceBus = serviceBus;
            this.Configuration = configuration ?? new AzurePlanningPokerConfigurationElement();
            this.NodeId = Guid.NewGuid().ToString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a controller of planning poker teams.
        /// </summary>
        /// <value>The planning poker controller.</value>
        public IAzurePlanningPoker PlanningPoker { get; private set; }

        /// <summary>
        /// Gets an ID of the Planning Poker node.
        /// </summary>
        public string NodeId { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets an Azure service bus object used to send messages between nodes.
        /// </summary>
        protected IServiceBus ServiceBus { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts synchronization with other nodes.
        /// </summary>
        public void Start()
        {
            this.ServiceBus.Register(this.NodeId);
            this.SetupPlanningPokerListeners();
            this.SetupServiceBusListeners();

            this.RequestTeamList();
        }

        /// <summary>
        /// Stops synchronization with other nodes.
        /// </summary>
        public void Stop()
        {
            if (this.sendNodeMessageSubscription != null)
            {
                this.sendNodeMessageSubscription.Dispose();
                this.sendNodeMessageSubscription = null;
            }

            if (this.serviceBusScrumTeamMessageSubscription != null)
            {
                this.serviceBusScrumTeamMessageSubscription.Dispose();
                this.serviceBusScrumTeamMessageSubscription = null;
            }

            if (this.serviceBusTeamCreatedMessageSubscription != null)
            {
                this.serviceBusTeamCreatedMessageSubscription.Dispose();
                this.serviceBusTeamCreatedMessageSubscription = null;
            }

            if (this.serviceBusRequestTeamListMessageSubscription != null)
            {
                this.serviceBusRequestTeamListMessageSubscription.Dispose();
                this.serviceBusRequestTeamListMessageSubscription = null;
            }

            if (this.serviceBusRequestTeamsMessageSubscription != null)
            {
                this.serviceBusRequestTeamsMessageSubscription.Dispose();
                this.serviceBusRequestTeamsMessageSubscription = null;
            }

            this.ServiceBus.Unregister();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged and optionally managed resources. 
        /// </summary>
        /// <param name="disposing"><c>True</c> if disposing not using GC; otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
            }
        }

        ~PlanningPokerAzureNode()
        {
            this.Dispose(false);
        }

        #endregion

        #region Private methods

        private void SetupPlanningPokerListeners()
        {
            var teamMessages = this.PlanningPoker.ObservableMessages.Where(m => !string.Equals(m.TeamName, this.processingScrumTeamName, StringComparison.OrdinalIgnoreCase));
            var nodeTeamMessages = teamMessages
                .Where(m => m.MessageType != MessageType.Empty && m.MessageType != MessageType.TeamCreated && m.MessageType != MessageType.EstimationEnded)
                .Select(m => new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = m });
            var createTeamMessages = teamMessages.Where(m => m.MessageType == MessageType.TeamCreated)
                .Select(m => this.CreateTeamCreatedMessage(m.TeamName));
            var nodeMessages = nodeTeamMessages.Merge(createTeamMessages);

            this.sendNodeMessageSubscription = nodeMessages.Subscribe(this.SendNodeMessage);
        }

        private void SetupServiceBusListeners()
        {
            var serviceBusMessages = this.ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, this.NodeId, StringComparison.OrdinalIgnoreCase));

            var busTeamMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.ScrumTeamMessage)
                .Select(m => (ScrumTeamMessage)m.Data);
            this.serviceBusScrumTeamMessageSubscription = busTeamMessages.Subscribe(this.ProcessTeamMessage);

            var busTeamCreatedMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.TeamCreated);
            this.serviceBusTeamCreatedMessageSubscription = busTeamCreatedMessages.Subscribe(this.OnScrumTeamCreated);
        }

        private NodeMessage CreateTeamCreatedMessage(string teamName)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                return new NodeMessage(NodeMessageType.TeamCreated)
                {
                    Data = ScrumTeamHelper.SerializeScrumTeam(team)
                };
            }
        }

        private void SendNodeMessage(NodeMessage message)
        {
            message.SenderNodeId = this.NodeId;
            this.ServiceBus.SendMessage(message);
        }

        #region ProcessTeamMessage

        private void OnScrumTeamCreated(NodeMessage message)
        {
            var scrumTeamData = (byte[])message.Data;
            var scrumTeam = ScrumTeamHelper.DeserializeScrumTeam(scrumTeamData, this.PlanningPoker.DateTimeProvider);
            try
            {
                this.processingScrumTeamName = scrumTeam.Name;
                using (var teamLock = this.PlanningPoker.AttachScrumTeam(scrumTeam))
                {
                }
            }
            finally
            {
                this.processingScrumTeamName = null;
            }
        }

        private void ProcessTeamMessage(ScrumTeamMessage message)
        {
            if (this.teamsToInitialize != null && !this.teamsToInitialize.Contains(message.TeamName, StringComparer.OrdinalIgnoreCase))
            {
                switch (message.MessageType)
                {
                    case MessageType.MemberJoined:
                        this.OnMemberJoinedMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                    case MessageType.MemberDisconnected:
                        this.OnMemberDisconnectedMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                    case MessageType.EstimationStarted:
                        this.OnEstimationStartedMessage(message.TeamName);
                        break;
                    case MessageType.EstimationCanceled:
                        this.OnEstimationCanceledMessage(message.TeamName);
                        break;
                    case MessageType.MemberEstimated:
                        this.OnMemberEstimatedMessage(message.TeamName, (ScrumTeamMemberEstimationMessage)message);
                        break;
                    case MessageType.MemberActivity:
                        this.OnMemberActivityMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                }
            }
        }

        private void OnMemberJoinedMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    team.Join(message.MemberName, string.Equals(message.MemberType, typeof(Observer).Name, StringComparison.OrdinalIgnoreCase));
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberDisconnectedMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    team.Disconnect(message.MemberName);
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        private void OnEstimationStartedMessage(string teamName)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    team.ScrumMaster.StartEstimation();
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        private void OnEstimationCanceledMessage(string teamName)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    team.ScrumMaster.CancelEstimation();
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberEstimatedMessage(string teamName, ScrumTeamMemberEstimationMessage message)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    var member = team.FindMemberOrObserver(message.MemberName) as Member;
                    if (member != null)
                    {
                        member.Estimation = new Estimation(message.Estimation);
                    }
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberActivityMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (var teamLock = this.PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                var team = teamLock.Team;
                try
                {
                    this.processingScrumTeamName = team.Name;
                    var observer = team.FindMemberOrObserver(message.MemberName);
                    if (observer != null)
                    {
                        observer.UpdateActivity();
                    }
                }
                finally
                {
                    this.processingScrumTeamName = null;
                }
            }
        }

        #endregion

        #region Process initialization messages

        private void RequestTeamList()
        {
            if (this.teamsToInitialize == null || this.teamsToInitialize.Count > 0)
            {
                var serviceBusMessages = this.ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, this.NodeId, StringComparison.OrdinalIgnoreCase));
                var teamListActions = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.TeamList).Take(1)
                    .Timeout(this.Configuration.InitializationMessageTimeout, Observable.Return<NodeMessage>(null))
                    .Select(m => new Action(() => this.ProcessTeamListMessage(m)));
                teamListActions.Subscribe(a => a());

                this.SendNodeMessage(new NodeMessage(NodeMessageType.RequestTeamList));
            }
            else
            {
                this.EndInitialization();
            }
        }

        private void ProcessTeamListMessage(NodeMessage message)
        {
            if (message != null)
            {
                var teamList = (IEnumerable<string>)message.Data;
                if (this.teamsToInitialize == null)
                {
                    this.teamsToInitialize = teamList.ToList();
                    this.PlanningPoker.SetTeamsInitializingList(this.teamsToInitialize);
                }

                this.RequestTeams(message.SenderNodeId);
            }
            else
            {
                this.EndInitialization();
            }
        }

        private void RequestTeams(string recipientId)
        {
            if (this.teamsToInitialize.Count == 0)
            {
                this.EndInitialization();
            }
            else
            {
                var lockObject = new object();
                var serviceBusMessages = this.ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, this.NodeId, StringComparison.OrdinalIgnoreCase));
                serviceBusMessages = serviceBusMessages.Synchronize(lockObject);

                var lastMessageTime = this.PlanningPoker.DateTimeProvider.UtcNow;

                var initTeamActions = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.InitializeTeam)
                    .TakeWhile(m => this.teamsToInitialize.Count > 0)
                    .Select(m => new Action(() => { lastMessageTime = this.PlanningPoker.DateTimeProvider.UtcNow; this.ProcessInitializeTeamMessage(m); }));
                var messageTimeoutActions = Observable.Interval(TimeSpan.FromSeconds(1.0)).Synchronize(lockObject)
                    .SelectMany(i => lastMessageTime + this.Configuration.InitializationMessageTimeout > this.PlanningPoker.DateTimeProvider.UtcNow ? Observable.Throw<Action>(new TimeoutException()) : Observable.Empty<Action>());

                initTeamActions.Merge(messageTimeoutActions)
                    .Subscribe(a => a(), e => this.RequestTeamList());

                var requestTeamsMessage = new NodeMessage(NodeMessageType.RequestTeams)
                {
                    RecipientNodeId = recipientId,
                    Data = this.teamsToInitialize.ToArray()
                };
                this.SendNodeMessage(requestTeamsMessage);
            }
        }

        private void ProcessInitializeTeamMessage(NodeMessage message)
        {
            var scrumTeamData = (byte[])message.Data;
            var scrumTeam = ScrumTeamHelper.DeserializeScrumTeam(scrumTeamData, this.PlanningPoker.DateTimeProvider);
            this.teamsToInitialize.Remove(scrumTeam.Name);
            this.PlanningPoker.InitializeScrumTeam(scrumTeam);

            if (this.teamsToInitialize.Count == 0)
            {
                this.EndInitialization();
            }
        }

        private void EndInitialization()
        {
            this.teamsToInitialize = new List<string>();
            this.PlanningPoker.EndInitialization();

            var serviceBusMessages = this.ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, this.NodeId, StringComparison.OrdinalIgnoreCase));

            var requestTeamListMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.RequestTeamList);
            this.serviceBusRequestTeamListMessageSubscription = requestTeamListMessages.Subscribe(this.ProcessRequestTeamListMesage);

            var requestTeamsMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.RequestTeams);
            this.serviceBusRequestTeamsMessageSubscription = requestTeamsMessages.Subscribe(this.ProcessRequestTeamsMessage);
        }

        private void ProcessRequestTeamListMesage(NodeMessage message)
        {
            var scrumTeamNames = this.PlanningPoker.ScrumTeamNames.ToArray();
            var teamListMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                RecipientNodeId = message.SenderNodeId,
                Data = scrumTeamNames
            };
            this.SendNodeMessage(teamListMessage);
        }

        private void ProcessRequestTeamsMessage(NodeMessage message)
        {
            var scrumTeamNames = (IEnumerable<string>)message.Data;
            foreach (var scrumTeamName in scrumTeamNames)
            {
                try
                {
                    byte[] scrumTeamData = null;
                    using (var teamLock = this.PlanningPoker.GetScrumTeam(scrumTeamName))
                    {
                        teamLock.Lock();
                        scrumTeamData = ScrumTeamHelper.SerializeScrumTeam(teamLock.Team);
                    }

                    var initializeTeamMessage = new NodeMessage(NodeMessageType.InitializeTeam)
                    {
                        RecipientNodeId = message.SenderNodeId,
                        Data = scrumTeamData
                    };
                    this.SendNodeMessage(initializeTeamMessage);
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

        #endregion
    }
}
