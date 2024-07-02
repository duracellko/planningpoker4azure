namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Type of message that can be sent to Scrum team members or observers.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Empty message that can be ignored. Used to notify member, that he/she should stop waiting for message.
    /// </summary>
    Empty,

    /// <summary>
    /// Message specifies that a new member joined Scrum team.
    /// </summary>
    MemberJoined,

    /// <summary>
    /// Message specifies that a member disconnected from Scrum team.
    /// </summary>
    MemberDisconnected,

    /// <summary>
    /// Message specifies that estimation started and members can pick estimation.
    /// </summary>
    EstimationStarted,

    /// <summary>
    /// Message specifies that estimation ended and all members picked their estimations.
    /// </summary>
    EstimationEnded,

    /// <summary>
    /// Message specifies that estimation was canceled by Scrum master.
    /// </summary>
    EstimationCanceled,

    /// <summary>
    /// Message specifies that a member placed estimation.
    /// </summary>
    MemberEstimated,

    /// <summary>
    /// Message specifies that a member is still active.
    /// </summary>
    MemberActivity,

    /// <summary>
    /// Message specifies that a new Scrum team was created.
    /// </summary>
    TeamCreated,

    /// <summary>
    /// Message specifies that the set of available estimations has changed.
    /// </summary>
    AvailableEstimationsChanged,

    /// <summary>
    /// Message specifies that a countdown timer for team has started.
    /// </summary>
    TimerStarted,

    /// <summary>
    /// Message specifies that a timer was canceled.
    /// </summary>
    TimerCanceled
}
