using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Health;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Manager of all Scrum teams playing planning poker on Windows Azure platform.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class AzurePlanningPokerController : PlanningPokerController, IAzurePlanningPoker, IInitializationStatusProvider, IDisposable
    {
        private readonly Subject<ScrumTeamMessage> _observableMessages = new Subject<ScrumTeamMessage>();
        private readonly object _teamsToInitializeLock = new object();
        private HashSet<string>? _teamsToInitialize;
        private volatile bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzurePlanningPokerController"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current date-time.</param>
        /// <param name="guidProvider">The GUID provider to provide new GUID objects.</param>
        /// <param name="deckProvider">The provider to get estimation cards deck.</param>
        /// <param name="configuration">The configuration of the planning poker.</param>
        /// <param name="repository">The Scrum teams repository.</param>
        /// <param name="taskProvider">The system tasks provider.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public AzurePlanningPokerController(
            DateTimeProvider? dateTimeProvider,
            GuidProvider? guidProvider,
            DeckProvider? deckProvider,
            IAzurePlanningPokerConfiguration? configuration,
            IScrumTeamRepository? repository,
            TaskProvider? taskProvider,
            ILogger<PlanningPokerController> logger)
            : base(dateTimeProvider, guidProvider, deckProvider, configuration, repository, taskProvider, logger)
        {
        }

        /// <summary>
        /// Gets an observable object sending messages from all Scrum teams.
        /// </summary>
        public IObservable<ScrumTeamMessage> ObservableMessages => _observableMessages;

        /// <summary>
        /// Gets a value indicating whether this instance of <see cref="AzurePlanningPokerController"/> is initialized.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Sets collection of Scrum team names, which exists in the Azure and need to be initialized in this node.
        /// </summary>
        /// <param name="teamNames">The list of team names.</param>
        public void SetTeamsInitializingList(IEnumerable<string> teamNames)
        {
            if (!_initialized)
            {
                lock (_teamsToInitializeLock)
                {
                    if (!_initialized)
                    {
                        Repository.DeleteAll();
                        _teamsToInitialize = new HashSet<string>(teamNames, StringComparer.OrdinalIgnoreCase);
                    }
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
                throw new ArgumentNullException(nameof(team));
            }

            if (!_initialized)
            {
                using (var teamLock = AttachScrumTeam(team))
                {
                    // The team can be released just after attaching.
                }

                lock (_teamsToInitializeLock)
                {
                    if (!_initialized)
                    {
                        if (_teamsToInitialize == null)
                        {
                            throw new InvalidOperationException(Resources.Error_InitializationIsNotStarted);
                        }

                        _teamsToInitialize.Remove(team.Name);
                        if (_teamsToInitialize.Count == 0)
                        {
                            _initialized = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Specifies that all teams are initialized and ready to use by this node.
        /// </summary>
        public void EndInitialization()
        {
            lock (_teamsToInitializeLock)
            {
                _initialized = true;
                _teamsToInitialize = null;
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                if (!_observableMessages.IsDisposed)
                {
                    _observableMessages.OnCompleted();
                    _observableMessages.Dispose();
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AzurePlanningPokerController"/> class.
        /// </summary>
        ~AzurePlanningPokerController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Executed when a Scrum team is added to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was added.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Null-check is in base method.")]
        protected override void OnTeamAdded(ScrumTeam team)
        {
            base.OnTeamAdded(team);
            team.MessageReceived += ScrumTeamOnMessageReceived;

            bool isInitializingTeam = false;
            if (!_initialized)
            {
                lock (_teamsToInitializeLock)
                {
                    isInitializingTeam = _teamsToInitialize != null && _teamsToInitialize.Contains(team.Name, StringComparer.OrdinalIgnoreCase);
                }
            }

            if (!isInitializingTeam)
            {
                var teamCreatedMessage = new ScrumTeamMessage(team.Name, MessageType.TeamCreated);
                _observableMessages.OnNext(teamCreatedMessage);
            }
        }

        /// <summary>
        /// Executed when a Scrum team is removed from collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was removed.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Null-check is in base method.")]
        protected override void OnTeamRemoved(ScrumTeam team)
        {
            team.MessageReceived -= ScrumTeamOnMessageReceived;
            base.OnTeamRemoved(team);
        }

        /// <summary>
        /// Executed before creating new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        protected override void OnBeforeCreateScrumTeam(string teamName, string scrumMasterName)
        {
            if (!_initialized)
            {
                bool teamListInitialized = false;
                lock (_teamsToInitializeLock)
                {
                    teamListInitialized = _teamsToInitialize != null;
                }

                if (!teamListInitialized)
                {
                    var timeout = InitializationTimeout;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!teamListInitialized && stopwatch.Elapsed < timeout)
                    {
                        System.Threading.Thread.Sleep(100);
                        lock (_teamsToInitializeLock)
                        {
                            teamListInitialized = _initialized || _teamsToInitialize != null;
                        }
                    }
                }

                lock (_teamsToInitializeLock)
                {
                    if (!_initialized)
                    {
                        if (_teamsToInitialize == null)
                        {
                            throw new TimeoutException();
                        }
                        else if (_teamsToInitialize.Contains(teamName))
                        {
                            throw new PlanningPokerException(Service.ErrorCodes.ScrumTeamAlreadyExists, teamName);
                        }
                    }
                }
            }

            base.OnBeforeCreateScrumTeam(teamName, scrumMasterName);
        }

        /// <summary>
        /// Executed before getting existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        protected override void OnBeforeGetScrumTeam(string teamName)
        {
            if (!_initialized)
            {
                bool teamInitialized = false;
                lock (_teamsToInitializeLock)
                {
                    teamInitialized = _teamsToInitialize != null && !_teamsToInitialize.Contains(teamName);
                }

                if (!teamInitialized)
                {
                    var timeout = InitializationTimeout;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!teamInitialized && stopwatch.Elapsed < timeout)
                    {
                        System.Threading.Thread.Sleep(100);
                        lock (_teamsToInitializeLock)
                        {
                            teamInitialized = _initialized || (_teamsToInitialize != null && !_teamsToInitialize.Contains(teamName));
                        }
                    }
                }
            }

            base.OnBeforeGetScrumTeam(teamName);
        }

        private TimeSpan InitializationTimeout
        {
            get
            {
                var configuration = Configuration as IAzurePlanningPokerConfiguration;
                return configuration != null ? configuration.InitializationTimeout : TimeSpan.FromMinutes(1.0);
            }
        }

        private void ScrumTeamOnMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            var team = (ScrumTeam)(sender ?? throw new ArgumentNullException(nameof(sender)));
            ScrumTeamMessage? scrumTeamMessage = null;

            switch (e.Message.MessageType)
            {
                case MessageType.MemberJoined:
                case MessageType.MemberDisconnected:
                case MessageType.MemberActivity:
                    var memberMessage = (MemberMessage)e.Message;
                    scrumTeamMessage = new ScrumTeamMemberMessage(team.Name, memberMessage.MessageType)
                    {
                        MemberName = memberMessage.Member.Name,
                        MemberType = memberMessage.Member.GetType().Name,
                        SessionId = memberMessage.Member.SessionId,
                        AcknowledgedMessageId = memberMessage.Member.AcknowledgedMessageId
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
                case MessageType.AvailableEstimationsChanged:
                    var estimationSetMessage = (EstimationSetMessage)e.Message;
                    scrumTeamMessage = new ScrumTeamEstimationSetMessage(team.Name, estimationSetMessage.MessageType)
                    {
                        Estimations = estimationSetMessage.Estimations.Select(e => e.Value).ToList()
                    };
                    break;
                case MessageType.TimerStarted:
                    var timerMessage = (TimerMessage)e.Message;
                    scrumTeamMessage = new ScrumTeamTimerMessage(team.Name, timerMessage.MessageType)
                    {
                        EndTime = timerMessage.EndTime
                    };
                    break;
                default:
                    scrumTeamMessage = new ScrumTeamMessage(team.Name, e.Message.MessageType);
                    break;
            }

            if (scrumTeamMessage != null)
            {
                _observableMessages.OnNext(scrumTeamMessage);
            }
        }
    }
}
