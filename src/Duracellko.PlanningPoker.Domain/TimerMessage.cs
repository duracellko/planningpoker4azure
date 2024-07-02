using System;
using Duracellko.PlanningPoker.Domain.Serialization;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Message sent to all members and observers, when a team member starts a countdown timer with specific end time.
/// </summary>
public class TimerMessage : Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimerMessage"/> class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="endTime">The time in UTC time zone that specifies, when countdown ends.</param>
    public TimerMessage(MessageType type, DateTime endTime)
        : base(type)
    {
        EndTime = endTime;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerMessage"/> class.
    /// </summary>
    /// <param name="messageData">Message serialization data.</param>
    internal TimerMessage(Serialization.MessageData messageData)
        : base(messageData)
    {
        EndTime = DateTime.SpecifyKind(messageData.EndTime!.Value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Gets the time in UTC time zone that specifies, when countdown ends.
    /// </summary>
    public DateTime EndTime { get; }

    /// <summary>
    /// Gets serialization data of the object.
    /// </summary>
    /// <returns>The serialization data.</returns>
    protected internal override MessageData GetData()
    {
        var result = base.GetData();
        result.EndTime = EndTime;
        return result;
    }
}
