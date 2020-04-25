using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum team is a group of members, who play planning poker, and observers, who watch the game.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Events are placed together with protected methods.")]
    public class ScrumTeam
    {
        private readonly List<Member> _members = new List<Member>();
        private readonly List<Observer> _observers = new List<Observer>();

        private readonly Estimation[] _availableEstimations = new Estimation[]
        {
            new Estimation(0.0),
            new Estimation(0.5),
            new Estimation(1.0),
            new Estimation(2.0),
            new Estimation(3.0),
            new Estimation(5.0),
            new Estimation(8.0),
            new Estimation(13.0),
            new Estimation(20.0),
            new Estimation(40.0),
            new Estimation(100.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
        };

        private EstimationResult _estimationResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        public ScrumTeam(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        public ScrumTeam(string name, DateTimeProvider dateTimeProvider)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            DateTimeProvider = dateTimeProvider ?? DateTimeProvider.Default;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="scrumTeamData">Scrum Team serialization data.</param>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        public ScrumTeam(Serialization.ScrumTeamData scrumTeamData, DateTimeProvider dateTimeProvider)
        {
            if (scrumTeamData == null)
            {
                throw new ArgumentNullException(nameof(scrumTeamData));
            }

            if (string.IsNullOrEmpty(scrumTeamData.Name))
            {
                throw new ArgumentException("Scrum Team name cannot be empty.", nameof(scrumTeamData));
            }

            DateTimeProvider = dateTimeProvider ?? DateTimeProvider.Default;
            Name = scrumTeamData.Name;
            State = scrumTeamData.State;

            if (scrumTeamData.Members != null)
            {
                DeserializeMembers(scrumTeamData);
            }

            DeserializeEstimationResult(scrumTeamData);

            if (scrumTeamData.Members != null)
            {
                foreach (var memberData in scrumTeamData.Members)
                {
                    var member = FindMemberOrObserver(memberData.Name);
                    member.DeserializeMessages(memberData);
                }
            }
        }

        /// <summary>
        /// Gets the Scrum team name.
        /// </summary>
        /// <value>The Scrum team name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the observers watching planning poker game of the Scrum team.
        /// </summary>
        /// <value>The observers collection.</value>
        public IEnumerable<Observer> Observers => _observers;

        /// <summary>
        /// Gets the collection members joined to the Scrum team.
        /// </summary>
        /// <value>The members collection.</value>
        public IEnumerable<Member> Members => _members;

        /// <summary>
        /// Gets the scrum master of the team.
        /// </summary>
        /// <value>The Scrum master.</value>
        public ScrumMaster ScrumMaster => Members.OfType<ScrumMaster>().FirstOrDefault();

        /// <summary>
        /// Gets the available estimations the members can pick from.
        /// </summary>
        /// <value>The collection of available estimations.</value>
        public IEnumerable<Estimation> AvailableEstimations => _availableEstimations;

        /// <summary>
        /// Gets the current Scrum team state.
        /// </summary>
        /// <value>The team state.</value>
        public TeamState State { get; private set; }

        /// <summary>
        /// Gets the estimation result, when <see cref="P:State"/> is EstimationFinished.
        /// </summary>
        /// <value>The estimation result.</value>
        public EstimationResult EstimationResult => State == TeamState.EstimationFinished ? _estimationResult : null;

        /// <summary>
        /// Gets the collection of participants in current estimation.
        /// </summary>
        /// <value>
        /// The estimation participants.
        /// </value>
        public IEnumerable<EstimationParticipantStatus> EstimationParticipants
        {
            get
            {
                if (State == TeamState.EstimationInProgress)
                {
                    return _estimationResult.Select(p => new EstimationParticipantStatus(p.Key.Name, p.Value != null)).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the date time provider used by the Scrum team.
        /// </summary>
        /// <value>The date-time provider.</value>
        public DateTimeProvider DateTimeProvider { get; }

        /// <summary>
        /// Sets new scrum master of the team.
        /// </summary>
        /// <param name="name">The Scrum master name.</param>
        /// <returns>The new Scrum master.</returns>
        public ScrumMaster SetScrumMaster(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberAlreadyExists, name), nameof(name));
            }

            if (ScrumMaster != null)
            {
                throw new InvalidOperationException(Resources.Error_ScrumMasterAlreadyExists);
            }

            var scrumMaster = new ScrumMaster(this, name);
            _members.Add(scrumMaster);

            var recipients = UnionMembersAndObservers().Where(m => m != scrumMaster);
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = scrumMaster });

            return scrumMaster;
        }

        /// <summary>
        /// Connects new member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <param name="asObserver">If set to <c>true</c> then connect new observer, otherwise member.</param>
        /// <returns>The observer or member, who joined the Scrum team.</returns>
        public Observer Join(string name, bool asObserver)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberAlreadyExists, name), nameof(name));
            }

            Observer result;
            if (asObserver)
            {
                var observer = new Observer(this, name);
                _observers.Add(observer);
                result = observer;
            }
            else
            {
                var member = new Member(this, name);
                _members.Add(member);
                result = member;
            }

            var recipients = UnionMembersAndObservers().Where(m => m != result);
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = result });

            return result;
        }

        /// <summary>
        /// Disconnects member with specified name from the Scrum team.
        /// </summary>
        /// <param name="name">The member name.</param>
        public void Disconnect(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Observer disconnectedObserver = null;
            var observer = _observers.FirstOrDefault(o => MatchObserverName(o, name));
            if (observer != null)
            {
                _observers.Remove(observer);
                disconnectedObserver = observer;
            }
            else
            {
                var member = _members.FirstOrDefault(o => MatchObserverName(o, name));
                if (member != null && !member.IsDormant)
                {
                    DisconnectMember(member);
                    disconnectedObserver = member;

                    if (State == TeamState.EstimationInProgress)
                    {
                        // Check if all members picked estimations. If member disconnects then his/her estimation is null.
                        UpdateEstimationResult(null);
                    }
                }
            }

            if (disconnectedObserver != null)
            {
                var recipients = UnionMembersAndObservers();
                SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = disconnectedObserver });

                // Send message to disconnecting member, so that he/she stops waiting for messages.
                disconnectedObserver.SendMessage(new Message(MessageType.Empty));
            }
        }

        /// <summary>
        /// Finds existing member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <returns>The member or observer.</returns>
        public Observer FindMemberOrObserver(string name)
        {
            var allObservers = UnionMembersAndObservers();
            return allObservers.FirstOrDefault(o => MatchObserverName(o, name));
        }

        /// <summary>
        /// Disconnects inactive observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        public void DisconnectInactiveObservers(TimeSpan inactivityTime)
        {
            var lastInactivityTime = DateTimeProvider.UtcNow - inactivityTime;
            bool IsObserverActive(Observer observer) => observer.LastActivity < lastInactivityTime && !observer.IsDormant;
            var inactiveObservers = Observers.Where(IsObserverActive).ToList();
            var inactiveMembers = Members.Where<Member>(IsObserverActive).ToList();

            if (inactiveObservers.Count > 0 || inactiveMembers.Count > 0)
            {
                foreach (var observer in inactiveObservers)
                {
                    _observers.Remove(observer);
                }

                foreach (var member in inactiveMembers)
                {
                    DisconnectMember(member);
                }

                var recipients = UnionMembersAndObservers();
                foreach (var member in inactiveObservers.Union(inactiveMembers))
                {
                    SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = member });
                }

                if (inactiveMembers.Count > 0)
                {
                    if (State == TeamState.EstimationInProgress)
                    {
                        // Check if all members picked estimations. If member disconnects then his/her estimation is null.
                        UpdateEstimationResult(null);
                    }
                }
            }
        }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        public virtual Serialization.ScrumTeamData GetData()
        {
            var result = new Serialization.ScrumTeamData
            {
                Name = Name,
                State = State,
            };

            result.Members = UnionMembersAndObservers().Select(m => m.GetData()).ToList();
            result.EstimationResult = _estimationResult?.GetData();
            return result;
        }

        /// <summary>
        /// Starts new estimation.
        /// </summary>
        internal void StartEstimation()
        {
            State = TeamState.EstimationInProgress;

            foreach (var member in Members)
            {
                member.ResetEstimation();
            }

            _estimationResult = new EstimationResult(Members);

            var recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new Message(MessageType.EstimationStarted));
        }

        /// <summary>
        /// Cancels current estimation.
        /// </summary>
        internal void CancelEstimation()
        {
            State = TeamState.EstimationCanceled;
            _estimationResult = null;

            var recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new Message(MessageType.EstimationCanceled));
        }

        /// <summary>
        /// Notifies that a member has placed estimation.
        /// </summary>
        /// <param name="member">The member, who estimated.</param>
        internal void OnMemberEstimated(Member member)
        {
            var recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberEstimated) { Member = member });
            UpdateEstimationResult(member);
        }

        /// <summary>
        /// Notifies that a member is still active.
        /// </summary>
        /// <param name="observer">The observer.</param>
        internal void OnObserverActivity(Observer observer)
        {
            SendMessage(new MemberMessage(MessageType.MemberActivity) { Member = observer });
        }

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private static bool MatchObserverName(Observer observer, string name)
        {
            return string.Equals(observer.Name, name, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<Observer> UnionMembersAndObservers()
        {
            foreach (var member in Members)
            {
                yield return member;
            }

            foreach (var observer in Observers)
            {
                yield return observer;
            }
        }

        private void SendMessage(Message message)
        {
            if (message != null)
            {
                OnMessageReceived(new MessageReceivedEventArgs(message));
            }
        }

        private void SendMessage(IEnumerable<Observer> recipients, Func<Message> messageFactory)
        {
            SendMessage(messageFactory());
            foreach (var recipient in recipients)
            {
                recipient.SendMessage(messageFactory());
            }
        }

        private void DisconnectMember(Member member)
        {
            if (member is ScrumMaster)
            {
                // Scrum Master is not removed from the team, because he is the owner.
                member.IsDormant = true;
            }
            else
            {
                _members.Remove(member);
            }
        }

        /// <summary>
        /// Checks if all members picked an estimation. If yes, then finishes the estimation.
        /// </summary>
        /// <param name="member">The who initiated member updating of estimation results.</param>
        private void UpdateEstimationResult(Member member)
        {
            if (member != null)
            {
                if (_estimationResult.ContainsMember(member))
                {
                    _estimationResult[member] = member.Estimation;
                }
            }

            if (_estimationResult.All(p => p.Value != null || !Members.Contains(p.Key)))
            {
                _estimationResult.SetReadOnly();
                State = TeamState.EstimationFinished;

                var recipients = UnionMembersAndObservers();
                SendMessage(recipients, () => new EstimationResultMessage(MessageType.EstimationEnded) { EstimationResult = _estimationResult });
            }
        }

        private void DeserializeMembers(Serialization.ScrumTeamData scrumTeamData)
        {
            var hasDuplicates = scrumTeamData.Members.GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);
            if (hasDuplicates)
            {
                throw new ArgumentException("Scrum Team member names must be unique.", nameof(scrumTeamData));
            }

            var scrumMasterData = scrumTeamData.Members.SingleOrDefault(m => m.MemberType == Serialization.MemberType.ScrumMaster);
            if (scrumMasterData != null)
            {
                _members.Add(new ScrumMaster(this, scrumMasterData));
            }

            foreach (var memberData in scrumTeamData.Members.Where(m => m.MemberType == Serialization.MemberType.Member))
            {
                _members.Add(new Member(this, memberData));
            }

            foreach (var observerData in scrumTeamData.Members.Where(m => m.MemberType == Serialization.MemberType.Observer))
            {
                _observers.Add(new Observer(this, observerData));
            }
        }

        private void DeserializeEstimationResult(Serialization.ScrumTeamData scrumTeamData)
        {
            if (scrumTeamData.EstimationResult != null)
            {
                _estimationResult = new EstimationResult(this, scrumTeamData.EstimationResult);

                if (State == TeamState.EstimationFinished)
                {
                    _estimationResult.SetReadOnly();
                }
            }
        }
    }
}
