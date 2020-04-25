using System;
using System.Collections.Generic;
using System.Linq;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Observer is not involved in estimations and cannot vote for estimation. However, he/she can watch planning poker and see estimations results.
    /// Usually product owner connects as an observer.
    /// </summary>
    public class Observer
    {
        private readonly Queue<Message> _messages = new Queue<Message>();
        private long _lastMessageId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Observer"/> class.
        /// </summary>
        /// <param name="team">The Scrum team the observer is joining.</param>
        /// <param name="name">The observer name.</param>
        public Observer(ScrumTeam team, string name)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Team = team;
            Name = name;
            LastActivity = Team.DateTimeProvider.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Observer"/> class.
        /// </summary>
        /// <param name="team">The Scrum team the observer is joining.</param>
        /// <param name="memberData">The member serialization data.</param>
        internal Observer(ScrumTeam team, Serialization.MemberData memberData)
        {
            if (string.IsNullOrEmpty(memberData.Name))
            {
                throw new ArgumentException("Member name cannot be empty.", nameof(memberData));
            }

            Team = team;
            Name = memberData.Name;
            LastActivity = DateTime.SpecifyKind(memberData.LastActivity, DateTimeKind.Utc);
            _lastMessageId = memberData.LastMessageId;
            IsDormant = memberData.IsDormant;
        }

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        public event EventHandler MessageReceived;

        /// <summary>
        /// Gets the Scrum team, the member is joined to.
        /// </summary>
        /// <value>The Scrum team.</value>
        public ScrumTeam Team { get; private set; }

        /// <summary>
        /// Gets the member's name.
        /// </summary>
        /// <value>The member's name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the member has any message.
        /// </summary>
        /// <value>
        ///     <c>True</c> if the member has message; otherwise, <c>false</c>.
        /// </value>
        public bool HasMessage => _messages.Count != 0;

        /// <summary>
        /// Gets the collection messages sent to the member.
        /// </summary>
        /// <value>The collection of messages.</value>
        public IEnumerable<Message> Messages => _messages;

        /// <summary>
        /// Gets the last time, the member checked for new messages.
        /// </summary>
        /// <value>The last activity time of the member.</value>
        public DateTime LastActivity { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the member is connected or not.
        /// This value is always false for regular members, because regular member is removed from team
        /// after disconnecting. However, Scrum Master is never removed from the team, because he is owner.
        /// This value tracks, if Scrum Master is actually connected or just dormant.
        /// </summary>
        /// <value>
        ///     <c>True</c> if member is connected; otherwise <c>false</c>.
        /// </value>
        public bool IsDormant { get; internal set; }

        /// <summary>
        /// Pops new message sent to the member and removes it from member's message queue.
        /// </summary>
        /// <returns>The new message or null, if there is no new message.</returns>
        public Message PopMessage()
        {
            return _messages.Count != 0 ? _messages.Dequeue() : null;
        }

        /// <summary>
        /// Clears the message queue.
        /// </summary>
        /// <returns>ID of last message sent to client.</returns>
        public long ClearMessages()
        {
            _messages.Clear();
            return _lastMessageId;
        }

        /// <summary>
        /// Updates time of <see cref="P:LastActivity"/> to current time.
        /// </summary>
        public void UpdateActivity()
        {
            IsDormant = false;
            LastActivity = Team.DateTimeProvider.UtcNow;
            Team.OnObserverActivity(this);
        }

        /// <summary>
        /// Sends the message to the member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        internal void SendMessage(Message message)
        {
            if (message != null)
            {
                _lastMessageId++;
                message.Id = _lastMessageId;
                _messages.Enqueue(message);
                OnMessageReceived(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Deserialize messages of member from serialized data.
        /// </summary>
        /// <param name="memberData">Serialized member data.</param>
        internal void DeserializeMessages(Serialization.MemberData memberData)
        {
            if (memberData.Messages != null)
            {
                foreach (var messageData in memberData.Messages)
                {
                    _messages.Enqueue(CreateMessage(messageData));
                }
            }
        }

        /// <summary>
        /// Gets serialization data of the object.
        /// </summary>
        /// <returns>The serialization data.</returns>
        protected internal virtual Serialization.MemberData GetData()
        {
            var result = new Serialization.MemberData
            {
                Name = Name,
                MemberType = Serialization.MemberType.Observer,
                LastActivity = LastActivity,
                LastMessageId = _lastMessageId,
                IsDormant = IsDormant,
            };

            result.Messages = Messages.Select(m => m.GetData()).ToList();
            return result;
        }

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(EventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, e);
            }
        }

        private Message CreateMessage(Serialization.MessageData messageData)
        {
            switch (messageData.MessageType)
            {
                case MessageType.Empty:
                case MessageType.EstimationStarted:
                case MessageType.EstimationCanceled:
                    return new Message(messageData);
                case MessageType.MemberJoined:
                case MessageType.MemberDisconnected:
                case MessageType.MemberEstimated:
                    var member = Team.FindMemberOrObserver(messageData.MemberName);
                    if (member == null)
                    {
                        member = new Member(Team, messageData.MemberName);
                    }

                    return new MemberMessage(messageData)
                    {
                        Member = member
                    };
                case MessageType.EstimationEnded:
                    var estimationResult = new EstimationResult(Team, messageData.EstimationResult);
                    estimationResult.SetReadOnly();
                    return new EstimationResultMessage(messageData)
                    {
                        EstimationResult = estimationResult
                    };
                default:
                    throw new ArgumentException($"Invalid message type {messageData.MessageType}.", nameof(messageData));
            }
        }
    }
}
