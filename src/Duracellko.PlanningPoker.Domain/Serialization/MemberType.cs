namespace Duracellko.PlanningPoker.Domain.Serialization;

/// <summary>
/// Type of Scrum Team member.
/// </summary>
public enum MemberType
{
    /// <summary>
    /// Observer is not involved in estimations and cannot vote for estimation.
    /// </summary>
    Observer,

    /// <summary>
    /// Member can vote in planning poker and can receive messages about planning poker game.
    /// </summary>
    Member,

    /// <summary>
    /// Scrum master can additionally to member start and cancel estimation planning poker.
    /// </summary>
    ScrumMaster
}
