// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Controllers
{
    /// <summary>
    /// Manager of all Scrum teams playing planning poker.
    /// </summary>
    public class PlanningPokerController : IPlanningPoker
    {
        #region Fields

        private readonly ConcurrentDictionary<string, Tuple<ScrumTeam, object>> scrumTeams = new ConcurrentDictionary<string, Tuple<ScrumTeam, object>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController"/> class.
        /// </summary>
        public PlanningPokerController()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController" /> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current date-time.</param>
        /// <param name="configuration">The configuration of the planning poker.</param>
        /// <param name="repository">The Scrum teams repository.</param>
        public PlanningPokerController(DateTimeProvider dateTimeProvider, IPlanningPokerConfiguration configuration, IScrumTeamRepository repository)
        {
            this.DateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
            this.Configuration = configuration ?? new PlanningPokerConfigurationElement();
            this.Repository = repository ?? new EmptyScrumTeamRepository();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the date time provider to provide current date-time.
        /// </summary>
        /// <value>The <see cref="DateTimeProvider"/> object.</value>
        public DateTimeProvider DateTimeProvider { get; private set; }

        /// <summary>
        /// Gets the configuration of planning poker.
        /// </summary>
        /// <value>The configuration.</value>
        public IPlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the team repository.
        /// </summary>
        /// <value>
        /// The team repository.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Properties in interface implementation.")]
        protected IScrumTeamRepository Repository { get; private set; }

        #endregion

        #region IPlanningPoker

        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        public IEnumerable<string> ScrumTeamNames
        {
            get
            {
                return this.scrumTeams.ToArray().Select(p => p.Key)
                    .Union(this.Repository.ScrumTeamNames.ToArray(), StringComparer.OrdinalIgnoreCase).ToArray();
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
        public IScrumTeamLock CreateScrumTeam(string teamName, string scrumMasterName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException("teamName");
            }

            if (string.IsNullOrEmpty(scrumMasterName))
            {
                throw new ArgumentNullException("scrumMasterName");
            }

            this.OnBeforeCreateScrumTeam(teamName, scrumMasterName);

            var team = new ScrumTeam(teamName, this.DateTimeProvider);
            team.SetScrumMaster(scrumMasterName);
            var teamLock = new object();
            var teamTuple = new Tuple<ScrumTeam, object>(team, teamLock);

            // loads team from repository and adds it to in-memory collection
            this.LoadScrumTeam(teamName);

            if (!this.scrumTeams.TryAdd(teamName, teamTuple))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ScrumTeamAlreadyExists, teamName), "teamName");
            }

            this.OnTeamAdded(team);

            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Adds existing Scrum team to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team to add.</param>
        /// <returns>The joined Scrum team.</returns>
        public IScrumTeamLock AttachScrumTeam(ScrumTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException("team");
            }

            var teamName = team.Name;
            var teamLock = new object();
            var teamTuple = new Tuple<ScrumTeam, object>(team, teamLock);

            // loads team from repository and adds it to in-memory collection
            this.LoadScrumTeam(teamName);

            if (!this.scrumTeams.TryAdd(teamName, teamTuple))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ScrumTeamAlreadyExists, teamName), "teamName");
            }

            this.OnTeamAdded(team);

            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Gets existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>
        /// The Scrum team.
        /// </returns>
        public IScrumTeamLock GetScrumTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException("teamName");
            }

            this.OnBeforeGetScrumTeam(teamName);

            Tuple<ScrumTeam, object> teamTuple = this.LoadScrumTeam(teamName);
            if (teamTuple == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ScrumTeamNotExist, teamName), "teamName");
            }

            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Calls specified callback when an observer receives new message or after configured timeout.
        /// </summary>
        /// <param name="observer">The observer to wait for message to receive.</param>
        /// <param name="callback">The callback delegate to call when a message is received or after timeout. First parameter specifies if message was received or not
        /// (the timeout occurs). Second parameter specifies observer, who received a message.</param>
        public void GetMessagesAsync(Observer observer, Action<bool, Observer> callback)
        {
            if (observer == null)
            {
                throw new ArgumentNullException("observer");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (observer.HasMessage)
            {
                callback(true, observer);
            }
            else
            {
                // not nead to load from repository, because team was already obtained
                Tuple<ScrumTeam, object> teamTuple;
                if (!this.scrumTeams.TryGetValue(observer.Team.Name, out teamTuple) || teamTuple.Item1 != observer.Team)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ScrumTeamNotExist, observer.Team.Name));
                }

                var messageReceivedObservable = Observable.FromEventPattern(h => observer.MessageReceived += h, h => observer.MessageReceived -= h);
                messageReceivedObservable = messageReceivedObservable.Timeout(this.Configuration.WaitForMessageTimeout, Observable.Return<System.Reactive.EventPattern<object>>(null));
                messageReceivedObservable = messageReceivedObservable.Take(1);
                messageReceivedObservable.Subscribe(p => ExecuteGetMessagesAsyncCallback(callback, observer, teamTuple));
            }
        }

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for configured period of time.
        /// </summary>
        public void DisconnectInactiveObservers()
        {
            this.DisconnectInactiveObservers(this.Configuration.ClientInactivityTimeout);
        }

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        public void DisconnectInactiveObservers(TimeSpan inactivityTime)
        {
            var teamTuples = this.scrumTeams.ToArray();
            foreach (var teamTuple in teamTuples)
            {
                using (var teamLock = new ScrumTeamLock(teamTuple.Value.Item1, teamTuple.Value.Item2))
                {
                    teamLock.Lock();
                    teamLock.Team.DisconnectInactiveObservers(inactivityTime);
                }
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Executed when a Scrum team is added to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was added.</param>
        protected virtual void OnTeamAdded(ScrumTeam team)
        {
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.ScrumTeamOnMessageReceived);
        }

        /// <summary>
        /// Executed when a Scrum team is removed from collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was removed.</param>
        protected virtual void OnTeamRemoved(ScrumTeam team)
        {
            team.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(this.ScrumTeamOnMessageReceived);
        }

        /// <summary>
        /// Executed before creating new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        protected virtual void OnBeforeCreateScrumTeam(string teamName, string scrumMasterName)
        {
            // empty implementation by default
        }

        /// <summary>
        /// Executed before getting existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        protected virtual void OnBeforeGetScrumTeam(string teamName)
        {
            // empty implementation by default
        }

        #endregion

        #region Private methods

        private static void ExecuteGetMessagesAsyncCallback(Action<bool, Observer> callback, Observer observer, Tuple<ScrumTeam, object> teamTuple)
        {
            using (var teamLock = new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2))
            {
                teamLock.Lock();
                callback(observer.HasMessage, observer.HasMessage ? observer : null);
            }
        }

        private void ScrumTeamOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var team = (ScrumTeam)sender;
            bool saveTeam = true;

            if (e.Message.MessageType == MessageType.MemberDisconnected)
            {
                if (!team.Members.Any() && !team.Observers.Any())
                {
                    saveTeam = false;
                    this.OnTeamRemoved(team);

                    Tuple<ScrumTeam, object> teamTuple;
                    this.scrumTeams.TryRemove(team.Name, out teamTuple);
                    this.Repository.DeleteScrumTeam(team.Name);
                }
            }

            if (saveTeam)
            {
                this.SaveScrumTeam(team);
            }
        }

        private Tuple<ScrumTeam, object> LoadScrumTeam(string teamName)
        {
            Tuple<ScrumTeam, object> result = null;
            bool retry = true;

            while (retry)
            {
                retry = false;

                if (!this.scrumTeams.TryGetValue(teamName, out result))
                {
                    result = null;

                    var team = this.Repository.LoadScrumTeam(teamName);
                    if (team != null)
                    {
                        if (this.VerifyTeamActive(team))
                        {
                            var teamLock = new object();
                            result = new Tuple<ScrumTeam, object>(team, teamLock);
                            if (this.scrumTeams.TryAdd(team.Name, result))
                            {
                                this.OnTeamAdded(team);
                            }
                            else
                            {
                                result = null;
                                retry = true;
                            }
                        }
                        else
                        {
                            this.Repository.DeleteScrumTeam(team.Name);
                        }
                    }
                }
            }

            return result;
        }

        private void SaveScrumTeam(ScrumTeam team)
        {
            this.Repository.SaveScrumTeam(team);
        }

        private bool VerifyTeamActive(ScrumTeam team)
        {
            team.DisconnectInactiveObservers(this.Configuration.ClientInactivityTimeout);
            return team.Members.Any() || team.Observers.Any();
        }

        #endregion

        #region Inner types

        /// <summary>
        /// Object used to lock Scrum team, so that only one thread can access the Scrum team at time.
        /// </summary>
        private sealed class ScrumTeamLock : IScrumTeamLock
        {
            #region Fields

            private readonly object lockObject;
            private bool locked;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="ScrumTeamLock"/> class.
            /// </summary>
            /// <param name="team">The Scrum team.</param>
            /// <param name="lockObj">The object used to lock the Scrum team.</param>
            public ScrumTeamLock(ScrumTeam team, object lockObj)
            {
                this.Team = team;
                this.lockObject = lockObj;
            }

            #endregion

            #region IScrumTeamLock

            /// <summary>
            /// Gets the Scrum team associated to the lock.
            /// </summary>
            /// <value>
            /// The Scrum team.
            /// </value>
            public ScrumTeam Team { get; private set; }

            /// <summary>
            /// Locks the Scrum team, so that other threads are not able to access the team.
            /// </summary>
            public void Lock()
            {
                if (!this.locked)
                {
                    Monitor.TryEnter(this.lockObject, 10000, ref this.locked);
                    if (!this.locked)
                    {
                        throw new TimeoutException(Properties.Resources.Error_ScrumTeamTimeout);
                    }
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (this.locked)
                {
                    Monitor.Exit(this.lockObject);
                }
            }

            #endregion
        }

        #endregion
    }
}
