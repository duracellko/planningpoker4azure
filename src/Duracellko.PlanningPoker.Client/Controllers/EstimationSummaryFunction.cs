namespace Duracellko.PlanningPoker.Client.Controllers;

/// <summary>
/// Type of function to calculate estimation summary value.
/// </summary>
public enum EstimationSummaryFunction
{
    /// <summary>
    /// Calculate average of all numeric estimation values.
    /// </summary>
    Average,

    /// <summary>
    /// Calculate median value of all numeric estimation values.
    /// </summary>
    Median,

    /// <summary>
    /// Calculate sum of all numeric estimation values.
    /// </summary>
    Sum
}
