using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class PlanningPokerController : INotifyPropertyChanged
    {
        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IBusyIndicatorService _busyIndicator;
        private List<MemberEstimation> _memberEstimations;
        private bool _hasJoinedEstimation;

        public PlanningPokerController(IPlanningPokerClient planningPokerService, IBusyIndicatorService busyIndicator)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _busyIndicator = busyIndicator ?? throw new ArgumentNullException(nameof(busyIndicator));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TeamMember User { get; private set; }

        public ScrumTeam ScrumTeam { get; private set; }

        public string TeamName => ScrumTeam?.Name;

        public long LastMessageId { get; private set; }

        public bool IsScrumMaster { get; private set; }

        public string ScrumMaster => ScrumTeam?.ScrumMaster?.Name;

        public IEnumerable<string> Members => ScrumTeam.Members
            .Where(m => m.Type != "ScrumMaster").Select(m => m.Name).OrderBy(m => m);

        public IEnumerable<string> Observers => ScrumTeam.Observers.Select(m => m.Name).OrderBy(m => m);

        public bool CanSelectEstimation => ScrumTeam.State == TeamState.EstimationInProgress && _hasJoinedEstimation;

        public bool CanStartEstimation => IsScrumMaster && ScrumTeam.State != TeamState.EstimationInProgress;

        public bool CanCancelEstimation => IsScrumMaster && ScrumTeam.State == TeamState.EstimationInProgress;

        public IEnumerable<double?> AvailableEstimations => ScrumTeam.AvailableEstimations.Select(e => e.Value)
            .OrderBy(v => v, EstimationComparer.Default);

        public IEnumerable<MemberEstimation> Estimations => _memberEstimations;

        public void InitializeTeam(ScrumTeam scrumTeam, string username)
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
            _memberEstimations = null;
            _hasJoinedEstimation = false;
        }

        public async Task SelectEstimation(double? estimation)
        {
            if (CanSelectEstimation)
            {
                using (_busyIndicator.Show())
                {
                    await _planningPokerService.SubmitEstimation(TeamName, User.Name, estimation, CancellationToken.None);
                }
            }
        }

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

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
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
            if (member.Type == "Observer")
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
        }

        private void OnEstimationEnded(EstimationResultMessage message)
        {
            _memberEstimations = message.EstimationResult.OrderBy(i => i.Estimation?.Value)
                .Select(i => new MemberEstimation(i.Member.Name, i.Estimation?.Value)).ToList();
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
