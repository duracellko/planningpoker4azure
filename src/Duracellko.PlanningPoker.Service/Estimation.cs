namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Estimation value of a planning poker card.
/// </summary>
public class Estimation
{
    /// <summary>
    /// Gets or sets the estimation value. Estimation can be any positive number (usually Fibonacci numbers) or
    /// positive infinity or null representing unknown estimation.
    /// </summary>
    /// <value>The estimation value.</value>
    public double? Value { get; set; }
}
