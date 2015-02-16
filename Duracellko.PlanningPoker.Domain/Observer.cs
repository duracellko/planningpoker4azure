// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Observer is not involved in estimations and cannot vote for estimation. However, he/she can watch planning poker and see estimations results.
    /// Usually product owner connects as an observer.
    /// </summary>
    [Serializable]
    public class Observer
    {
        #region Fields

        private readonly Queue<Message> messages = new Queue<Message>();
        private long lastMessageId;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Observer"/> class.
        /// </summary>
        /// <param name="team">The Scrum team the observer is joining.</param>
        /// <param name="name">The observer name.</param>
        public Observer(ScrumTeam team, string name)
        {
            if (team == null)
            {
                throw new ArgumentNullException("team");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.Team = team;
            this.Name = name;
            this.LastActivity = this.Team.DateTimeProvider.UtcNow;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler MessageReceived;

        #endregion

        #region Properties

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
        public bool HasMessage
        {
            get
            {
                return this.messages.Count != 0;
            }
        }

        /// <summary>
        /// Gets the collection messages sent to the member.
        /// </summary>
        /// <value>The collection of messages.</value>
        public IEnumerable<Message> Messages
        {
            get
            {
                return this.messages;
            }
        }

        /// <summary>
        /// Gets the last time, the member checked for new messages.
        /// </summary>
        /// <value>The last activity time of the member.</value>
        public DateTime LastActivity { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Pops new message sent to the member and removes it from member's message queue.
        /// </summary>
        /// <returns>The new message or null, if there is no new message.</returns>
        public Message PopMessage()
        {
            return this.messages.Count != 0 ? this.messages.Dequeue() : null;
        }

        /// <summary>
        /// Clears the message queue.
        /// </summary>
        /// <returns>ID of last message sent to client.</returns>
        public long ClearMessages()
        {
            this.messages.Clear();
            return this.lastMessageId;
        }

        /// <summary>
        /// Updates time of <see cref="P:LastActivity"/> to current time.
        /// </summary>
        public void UpdateActivity()
        {
            this.LastActivity = this.Team.DateTimeProvider.UtcNow;
            this.Team.OnObserverActivity(this);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Sends the message to the member.
        /// </summary>
        /// <param name="message">The message to send.</param>
        internal void SendMessage(Message message)
        {
            if (message != null)
            {
                this.lastMessageId++;
                message.Id = this.lastMessageId;
                this.messages.Enqueue(message);
                this.OnMessageReceived(EventArgs.Empty);
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(EventArgs e)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(this, e);
            }
        }

        #endregion
    }
}
