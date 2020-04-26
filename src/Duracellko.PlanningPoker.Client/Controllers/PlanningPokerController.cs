using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Manages state of planning poker game and provides data for view.
    /// </summary>
    public class PlanningPokerController : IPlanningPokerInitializer, INotifyPropertyChanged
    {
        private const string ScrumMasterType = "ScrumMaster";
        private const string ObserverType = "Observer";

        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IBusyIndicatorService _busyIndicator;
        private readonly IMemberCredentialsStore _memberCredentialsStore;
        private List<MemberEstimation> _memberEstimations;
        private bool _isConnected;
        private bool _hasJoinedEstimation;
        private Estimation _selectedEstimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController" /> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to send messages to server.</param>
        /// <param name="busyIndicator">Service to show busy indicator, when operation is in progress.</param>
        /// <param name="memberCredentialsStore">Service to save and load member credentials.</param>
        public PlanningPokerController(
            IPlanningPokerClient planningPokerService,
            IBusyIndicatorService busyIndicator,
            IMemberCredentialsStore memberCredentialsStore)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _busyIndicator = busyIndicator ?? throw new ArgumentNullException(nameof(busyIndicator));
            _memberCredentialsStore = memberCredentialsStore ?? throw new ArgumentNullException(nameof(memberCredentialsStore));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets current user joining planning poker.
        /// </summary>
        public TeamMember User { get; private set; }

        /// <summary>
        /// Gets Scrum Team data received from server.
        /// </summary>
        public ScrumTeam ScrumTeam { get; private set; }

        /// <summary>
        /// Gets Scrum Team name.
        /// </summary>
        public string TeamName => ScrumTeam?.Name;

        /// <summary>
        /// Gets a value indicating whether current user is connected to Planning Poker game.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsConnected)));
                }
            }
        }

        /// <summary>
        /// Gets ID of last received message.
        /// </summary>
        public long LastMessageId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether current user is Scrum Master and can start or stop estimation.
        /// </summary>
        public bool IsScrumMaster { get; private set; }

        /// <summary>
        /// Gets name of Scrum Master.
        /// </summary>
        public MemberItem ScrumMaster => ScrumTeam?.ScrumMaster != null ? new MemberItem(ScrumTeam.ScrumMaster, false) : null;

        /// <summary>
        /// Gets collection of member names, who can estimate.
        /// </summary>
        public IEnumerable<MemberItem> Members => ScrumTeam.Members
            .Where(m => m.Type != ScrumMasterType)
            .Select(m => new MemberItem(m, false))
            .OrderBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets collection of observer names, who just observe estimation.
        /// </summary>
        public IEnumerable<MemberItem> Observers => ScrumTeam.Observers
            .Select(m => new MemberItem(m, false))
            .OrderBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether user can estimate and estimation is in progress.
        /// </summary>
        public bool CanSelectEstimation => ScrumTeam.State == TeamState.EstimationInProgress &&
            User.Type != ObserverType && _hasJoinedEstimation && _selectedEstimation == null;

        /// <summary>
        /// Gets a value indicating whether user can start estimation.
        /// </summary>
        public bool CanStartEstimation => IsScrumMaster && ScrumTeam.State != TeamState.EstimationInProgress;

        /// <summary>
        /// Gets a value indicating whether user can cancel estimation.
        /// </summary>
        public bool CanCancelEstimation => IsScrumMaster && ScrumTeam.State == TeamState.EstimationInProgress;

        /// <summary>
        /// Gets a collection of available estimation values, user can select from.
        /// </summary>
        public IEnumerable<double?> AvailableEstimations => ScrumTeam.AvailableEstimations.Select(e => e.Value)
            .OrderBy(v => v, EstimationComparer.Default);

        /// <summary>
        /// Gets a collection of selected estimates by all users.
        /// </summary>
        public IEnumerable<MemberEstimation> Estimations => _memberEstimations;

        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="scrumTeam">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        /// <returns>Asynchronous operation.</returns>
        public Task InitializeTeam(ScrumTeam scrumTeam, string username)
        {
            if (scrumTeam == null)
            {
                throw new ArgumentNullException(nameof(scrumTeam));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (scrumTeam.Members == null)
            {
                scrumTeam.Members = new List<TeamMember>();
            }

            if (scrumTeam.Observers == null)
            {
                scrumTeam.Observers = new List<TeamMember>();
            }

            ScrumTeam = scrumTeam;
            User = FindTeamMember(username);
            IsScrumMaster = User != null && User == ScrumTeam.ScrumMaster;
            LastMessageId = -1;

            if (scrumTeam.EstimationResult != null)
            {
                _memberEstimations = GetMemberEstimationList(scrumTeam.EstimationResult);
            }
            else if (scrumTeam.EstimationParticipants != null)
            {
                _memberEstimations = scrumTeam.EstimationParticipants
                    .Where(p => p.Estimated).Select(p => new MemberEstimation(p.MemberName)).ToList();
            }
            else
            {
                _memberEstimations = null;
            }

            IsConnected = true;
            _hasJoinedEstimation = scrumTeam.EstimationParticipants != null &&
                scrumTeam.EstimationParticipants.Any(p => string.Equals(p.MemberName, User?.Name, StringComparison.OrdinalIgnoreCase));
            _selectedEstimation = null;

            var memberCredentials = new MemberCredentials
            {
                TeamName = TeamName,
                MemberName = User.Name
            };
            return _memberCredentialsStore.SetCredentialsAsync(memberCredentials);
        }

        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="teamInfo">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        /// <returns>Asynchronous operation.</returns>
        /// <remarks>This method overloads setup additional information after reconnecting to existing team.</remarks>
        public async Task InitializeTeam(ReconnectTeamResult teamInfo, string username)
        {
            if (teamInfo == null)
            {
                throw new ArgumentNullException(nameof(teamInfo));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            await InitializeTeam(teamInfo.ScrumTeam, username);

            LastMessageId = teamInfo.LastMessageId;
            _selectedEstimation = teamInfo.SelectedEstimation;
        }

        /// <summary>
        /// Disconnect current user from current Scrum Team.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task Disconnect()
        {
            using (_busyIndicator.Show())
            {
                await _planningPokerService.DisconnectTeam(TeamName, User.Name, CancellationToken.None);
                IsConnected = false;
                await _memberCredentialsStore.SetCredentialsAsync(null);
            }
        }

        /// <summary>
        /// Disconnects member from existing Planning Poker game. This functionality can be used by ScrumMaster only.
        /// </summary>
        /// <param name="member">Name of member to disconnect.</param>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task DisconnectMember(string member)
        {
            if (string.IsNullOrEmpty(member))
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (string.Equals(member, User.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("ScrumMaster cannot disconnect himself.", nameof(member));
            }

            using (_busyIndicator.Show())
            {
                await _planningPokerService.DisconnectTeam(TeamName, member, CancellationToken.None);
            }
        }

        /// <summary>
        /// Selects estimation by user, when estimation is in progress.
        /// </summary>
        /// <param name="estimation">Selected estimation value.</param>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task SelectEstimation(double? estimation)
        {
            if (CanSelectEstimation)
            {
                using (_busyIndicator.Show())
                {
                    var selectedEstimation = ScrumTeam.AvailableEstimations.First(e => e.Value == estimation);
                    await _planningPokerService.SubmitEstimation(TeamName, User.Name, estimation, CancellationToken.None);
                    _selectedEstimation = selectedEstimation;
                }
            }
        }

        /// <summary>
        /// Starts estimation by Scrum Master.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task StartEstimation()
        {
            if (CanStartEstimation)
            {
                using (_busyIndicator.Show())
                {
                    await _planningPokerService.StartEstimation(TeamName, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Stops estimation by Scrum Master.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task CancelEstimation()
        {
            if (CanCancelEstimation)
            {
                using (_busyIndicator.Show())
                {
                    await _planningPokerService.CancelEstimation(TeamName, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Processes messages received from server and updates status of planning poker game.
        /// </summary>
        /// <param name="messages">Collection of messages received from server.</param>
        public void ProcessMessages(IEnumerable<Message> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            foreach (var message in messages.OrderBy(m => m.Id))
            {
                ProcessMessage(message);
            }
        }

        /// <summary>
        /// Notifies that a property of this instance has been changed.
        /// </summary>
        /// <param name="e">Arguments of PropertyChanged event.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private static List<MemberEstimation> GetMemberEstimationList(IList<EstimationResultItem> estimationResult)
        {
            var estimationValueCounts = new Dictionary<double, int>();
            foreach (var estimation in estimationResult)
            {
                if (estimation.Estimation != null)
                {
                    double key = GetEstimationValueKey(estimation.Estimation.Value);
                    if (estimationValueCounts.TryGetValue(key, out int count))
                    {
                        estimationValueCounts[key] = count + 1;
                    }
                    else
                    {
                        estimationValueCounts.Add(key, 1);
                    }
                }
            }

            return estimationResult
                .OrderByDescending(i => i.Estimation != null ? estimationValueCounts[GetEstimationValueKey(i.Estimation.Value)] : 0)
                .ThenBy(i => i.Estimation?.Value, EstimationComparer.Default)
                .ThenBy(i => i.Member.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(i => i.Estimation != null ? new MemberEstimation(i.Member.Name, i.Estimation.Value) : new MemberEstimation(i.Member.Name))
                .ToList();
        }

        private static double GetEstimationValueKey(double? value)
        {
            if (!value.HasValue)
            {
                return -1111111.0;
            }
            else if (double.IsPositiveInfinity(value.Value))
            {
                return -1111100.0;
            }
            else
            {
                return value.Value;
            }
        }

        private void ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.MemberJoined:
                    OnMemberJoined((MemberMessage)message);
                    break;
                case MessageType.MemberDisconnected:
                    OnMemberDisconnected((MemberMessage)message);
                    break;
                case MessageType.EstimationStarted:
                    OnEstimationStarted();
                    break;
                case MessageType.EstimationEnded:
                    OnEstimationEnded((EstimationResultMessage)message);
                    break;
                case MessageType.EstimationCanceled:
                    OnEstimationCanceled();
                    break;
                case MessageType.MemberEstimated:
                    OnMemberEstimated((MemberMessage)message);
                    break;
            }

            LastMessageId = message.Id;
            OnPropertyChanged(new PropertyChangedEventArgs(null));
        }

        private void OnMemberJoined(MemberMessage message)
        {
            var member = message.Member;
            if (member.Type == ObserverType)
            {
                ScrumTeam.Observers.Add(member);
            }
            else
            {
                ScrumTeam.Members.Add(member);
            }
        }

        private void OnMemberDisconnected(MemberMessage message)
        {
            var name = message.Member.Name;
            if (ScrumTeam.ScrumMaster != null &&
                string.Equals(ScrumTeam.ScrumMaster.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                ScrumTeam.ScrumMaster = null;
            }
            else
            {
                var member = ScrumTeam.Members.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                {
                    ScrumTeam.Members.Remove(member);
                }
                else
                {
                    member = ScrumTeam.Observers.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (member != null)
                    {
                        ScrumTeam.Observers.Remove(member);
                    }
                }
            }
        }

        private void OnEstimationStarted()
        {
            _memberEstimations = new List<MemberEstimation>();
            ScrumTeam.State = TeamState.EstimationInProgress;
            _hasJoinedEstimation = true;
            _selectedEstimation = null;
        }

        private void OnEstimationEnded(EstimationResultMessage message)
        {
            _memberEstimations = GetMemberEstimationList(message.EstimationResult);
            ScrumTeam.State = TeamState.EstimationFinished;
        }

        private void OnEstimationCanceled()
        {
            ScrumTeam.State = TeamState.EstimationCanceled;
        }

        private void OnMemberEstimated(MemberMessage message)
        {
            if (_memberEstimations != null && !string.IsNullOrEmpty(message.Member?.Name))
            {
                _memberEstimations.Add(new MemberEstimation(message.Member.Name));
            }
        }

        private TeamMember FindTeamMember(string name)
        {
            if (ScrumTeam.ScrumMaster != null &&
                string.Equals(ScrumTeam.ScrumMaster.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return ScrumTeam.ScrumMaster;
            }

            var result = ScrumTeam.Members.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (result != null)
            {
                return result;
            }

            result = ScrumTeam.Observers.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private class EstimationComparer : IComparer<double?>
        {
            public static EstimationComparer Default { get; } = new EstimationComparer();

            public int Compare(double? x, double? y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null)
                {
                    return 1;
                }
                else if (y == null)
                {
                    return -1;
                }
                else
                {
                    return Comparer<double>.Default.Compare(x.Value, y.Value);
                }
            }
        }
    }
}
