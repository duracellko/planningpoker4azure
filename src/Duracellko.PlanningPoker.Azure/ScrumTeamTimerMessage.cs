using System;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure;

/// <summary>
/// Message of event in specific Scrum team that countdown timer has started.
/// </summary>
public class ScrumTeamTimerMessage : ScrumTeamMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamTimerMessage"/> class.
    /// </summary>
    public ScrumTeamTimerMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamTimerMessage"/> class.
    /// </summary>
    /// <param name="teamName">The name of team, this message is related to.</param>
    /// <param name="messageType">The type of message.</param>
    public ScrumTeamTimerMessage(string teamName, MessageType messageType)
        : base(teamName, messageType)
    {
    }

    /// <summary>
    /// Gets or sets the time in UTC time zone that specifies, when countdown ends.
    /// </summary>
    public DateTime EndTime { get; set; }
}
