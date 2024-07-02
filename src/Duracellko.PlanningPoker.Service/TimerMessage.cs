using System;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Message sent to all members and observers, when a team member starts a countdown timer with specific end time.
/// </summary>
public class TimerMessage : Message
{
    /// <summary>
    /// Gets or sets the time in UTC time zone that specifies, when countdown ends.
    /// </summary>
    public DateTime EndTime { get; set; }
}
