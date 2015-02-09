// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Manager of all Scrum teams playing planning poker on Windows Azure platform.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class AzurePlanningPokerController : PlanningPokerController, IAzurePlanningPoker, IDisposable
    {
        #region Fields

        private readonly Subject<ScrumTeamMessage> observableMessages = new Subject<ScrumTeamMessage>();
        private HashSet<string> teamsToInitialize;
        private object teamsToInitializeLock = new object();
        private volatile bool initialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePlanningPokerController"/> class.
        /// </summary>
        public AzurePlanningPokerController()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePlanningPokerController"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current date-time.</param>
        /// <param name="configuration">The configuration of the planning poker.</param>
        public AzurePlanningPokerController(DateTimeProvider dateTimeProvider, IAzurePlanningPokerConfiguration configuration)
            : base(dateTimeProvider, configuration)
        {
        }

        #endregion

        #region IAzurePlanningPoker

        /// <summary>
        /// Gets an observable object sending messages from all Scrum teams.
        /// </summary>
        public IObservable<ScrumTeamMessage> ObservableMessages
        {
            get
            {
                return this.observableMessages;
            }
        }

        /// <summary>
        /// Sets collection of Scrum team names, which exists in the Azure and need to be initialized in this node.
        /// </summary>
        /// <param name="teamNames">The list of team names.</param>
        public void SetTeamsInitializingList(IEnumerable<string> teamNames)
        {
            if (!this.initialized)
            {
                lock (this.teamsToInitializeLock)
                {
                    this.teamsToInitialize = new HashSet<string>(teamNames, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        /// <summary>
        /// Inserts existing Scrum team into collection and marks the team as initialized in this node.
        /// </summary>
        /// <param name="team">The Scrum team to insert.</param>
        public void InitializeScrumTeam(ScrumTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException("team");
            }

            if (!this.initialized)
            {
                using (var teamLock = this.AttachScrumTeam(team))
                {
                }

                lock (this.teamsToInitializeLock)
                {
                    this.teamsToInitialize.Remove(team.Name);
                    if (this.teamsToInitialize.Count == 0)
                    {
                        this.initialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Specifies that all teams are initialized and ready to use by this node.
        /// </summary>
        public void EndInitialization()
        {
            this.initialized = true;
            lock (this.teamsToInitializeLock)
            {
                this.teamsToInitialize = null;
            }
        }

        /// <summary>
        /// Creates new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>
        /// The new Scrum team.
        /// </returns>
        public override IScrumTeamLock CreateScrumTeam(string teamName, string scrumMasterName)
        {
            if (!this.initialized)
            {
                bool teamListInitialized = false;
                lock (this.teamsToInitializeLock)
                {
                    teamListInitialized = this.teamsToInitialize != null;
                }

                if (!teamListInitialized)
                {
                    var timeout = this.InitializationTimeout;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!teamListInitialized && stopwatch.Elapsed < timeout)
                    {
                        System.Threading.Thread.Sleep(100);
                        lock (this.teamsToInitializeLock)
                        {
                            teamListInitialized = this.initialized || this.teamsToInitialize != null;
                        }
                    }
                }

                lock (this.teamsToInitializeLock)
                {
                    if (!this.initialized)
                    {
                        if (this.teamsToInitialize == null)
                        {
                            throw new TimeoutException();
                        }
                        else if (this.teamsToInitialize.Contains(teamName))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ScrumTeamAlreadyExists, teamName), "teamName");
                        }
                    }
                }
            }

            return base.CreateScrumTeam(teamName, scrumMasterName);
        }

        /// <summary>
        /// Gets existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>
        /// The Scrum team.
        /// </returns>
        public override IScrumTeamLock GetScrumTeam(string teamName)
        {
            if (!this.initialized)
            {
                bool teamInitialized = false;
                lock (this.teamsToInitializeLock)
                {
                    teamInitialized = this.teamsToInitialize != null && !this.teamsToInitialize.Contains(teamName);
                }

                if (!teamInitialized)
                {
                    var timeout = this.InitializationTimeout;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!teamInitialized && stopwatch.Elapsed < timeout)
                    {
                        System.Threading.Thread.Sleep(100);
                        lock (this.teamsToInitializeLock)
                        {
                            teamInitialized = this.initialized || (this.teamsToInitialize != null && !this.teamsToInitialize.Contains(teamName));
                        }
                    }
                }
            }

            return base.GetScrumTeam(teamName);
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
                this.observableMessages.OnCompleted();
                this.observableMessages.Dispose();
            }
        }

        ~AzurePlanningPokerController()
        {
            this.Dispose(false);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Executed when a Scrum team is added to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was added.</param>
        protected override void OnTeamAdded(ScrumTeam team)
        {
            base.OnTeamAdded(team);
            team.MessageReceived += this.ScrumTeamOnMessageReceived;

            bool isInitializingTeam = false;
            if (!this.initialized)
            {
                lock (this.teamsToInitializeLock)
                {
                    isInitializingTeam = this.teamsToInitialize != null && this.teamsToInitialize.Contains(team.Name, StringComparer.OrdinalIgnoreCase);
                }
            }

            if (!isInitializingTeam)
            {
                var teamCreatedMessage = new ScrumTeamMessage(team.Name, MessageType.TeamCreated);
                this.observableMessages.OnNext(teamCreatedMessage);
            }
        }

        /// <summary>
        /// Executed when a Scrum team is removed from collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was removed.</param>
        protected override void OnTeamRemoved(ScrumTeam team)
        {
            team.MessageReceived -= this.ScrumTeamOnMessageReceived;
            base.OnTeamRemoved(team);
        }

        #endregion

        #region Properties properties

        private TimeSpan InitializationTimeout
        {
            get
            {
                var configuration = this.Configuration as IAzurePlanningPokerConfiguration;
                return configuration != null ? configuration.InitializationTimeout : TimeSpan.FromMinutes(1.0);
            }
        }

        #endregion

        #region Private methods

        private void ScrumTeamOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var team = (ScrumTeam)sender;
            ScrumTeamMessage scrumTeamMessage = null;

            switch (e.Message.MessageType)
            {
                case MessageType.MemberJoined:
                case MessageType.MemberDisconnected:
                case MessageType.MemberActivity:
                    var memberMessage = (MemberMessage)e.Message;
                    scrumTeamMessage = new ScrumTeamMemberMessage(team.Name, memberMessage.MessageType)
                    {
                        MemberName = memberMessage.Member.Name,
                        MemberType = memberMessage.Member.GetType().Name
                    };
                    break;
                case MessageType.MemberEstimated:
                    var memberEstimatedMessage = (MemberMessage)e.Message;
                    var member = memberEstimatedMessage.Member as Member;
                    if (member != null && member.Estimation != null)
                    {
                        scrumTeamMessage = new ScrumTeamMemberEstimationMessage(team.Name, memberEstimatedMessage.MessageType)
                        {
                            MemberName = member.Name,
                            Estimation = member.Estimation.Value
                        };
                    }

                    break;
                default:
                    scrumTeamMessage = new ScrumTeamMessage(team.Name, e.Message.MessageType);
                    break;
            }

            if (scrumTeamMessage != null)
            {
                this.observableMessages.OnNext(scrumTeamMessage);
            }
        }

        #endregion
    }
}
