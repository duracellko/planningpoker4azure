namespace Duracellko.PlanningPoker.Client.Controllers;

/// <summary>
/// Object contains information about member estimates.
/// </summary>
public class MemberEstimation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberEstimation" /> class.
    /// </summary>
    /// <param name="memberName">Name of member, who estimated.</param>
    /// <remarks>Estimated value is not disclosed.</remarks>
    public MemberEstimation(string memberName)
    {
        MemberName = memberName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberEstimation" /> class.
    /// </summary>
    /// <param name="memberName">Name of member, who estimated.</param>
    /// <param name="estimation">Estimated value.</param>
    public MemberEstimation(string memberName, double? estimation)
        : this(memberName)
    {
        Estimation = estimation;
        HasEstimation = true;
    }

    /// <summary>
    /// Gets name of member, who estimated.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets a value indicating whether estimation is disclosed.
    /// </summary>
    public bool HasEstimation { get; }

    /// <summary>
    /// Gets estimated value.
    /// </summary>
    public double? Estimation { get; }
}
