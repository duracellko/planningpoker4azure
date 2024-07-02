namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Status of participant in estimation.
/// </summary>
public class EstimationParticipantStatus
{
    /// <summary>
    /// Gets or sets the name of the participant.
    /// </summary>
    /// <value>
    /// The name of the member.
    /// </value>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this participant submitted an estimate already.
    /// </summary>
    /// <value>
    /// <c>True</c> if participant estimated; otherwise, <c>false</c>.
    /// </value>
    public bool Estimated { get; set; }
}
