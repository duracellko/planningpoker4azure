using System;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Message of event in specific Scrum team.
    /// </summary>
    public class ScrumTeamMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMessage"/> class.
        /// </summary>
        public ScrumTeamMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMessage"/> class.
        /// </summary>
        /// <param name="teamName">The name of team, this message is related to.</param>
        /// <param name="messageType">The type of message.</param>
        public ScrumTeamMessage(string teamName, MessageType messageType)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            TeamName = teamName;
            MessageType = messageType;
        }

        /// <summary>
        /// Gets or sets a name of team, which this message is related to.
        /// </summary>
        /// <value>The Scrum team name.</value>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a type of message sent by a Scrum team.
        /// </summary>
        /// <value>The message type.</value>
        public MessageType MessageType { get; set; }
    }
}
