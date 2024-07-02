using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure;

/// <summary>
/// Message of event in specific Scrum team, which notifies that a member placed estimation.
/// </summary>
public class ScrumTeamMemberEstimationMessage : ScrumTeamMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimationMessage"/> class.
    /// </summary>
    public ScrumTeamMemberEstimationMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimationMessage"/> class.
    /// </summary>
    /// <param name="teamName">The name of team, this message is related to.</param>
    /// <param name="messageType">The type of message.</param>
    public ScrumTeamMemberEstimationMessage(string teamName, MessageType messageType)
        : base(teamName, messageType)
    {
    }

    /// <summary>
    /// Gets or sets a name of member, which this message is related to.
    /// </summary>
    /// <value>The member name.</value>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a member's estimation.
    /// </summary>
    /// <value>The member's estimation.</value>
    public double? Estimation { get; set; }
}
