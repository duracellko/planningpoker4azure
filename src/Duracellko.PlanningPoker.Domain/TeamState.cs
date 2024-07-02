namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Specifies status if Scrum team.
/// </summary>
public enum TeamState
{
    /// <summary>
    /// Scrum team is initial state and estimation has not started yet.
    /// </summary>
    Initial,

    /// <summary>
    /// Estimation is in progress. Members can pick their estimations.
    /// </summary>
    EstimationInProgress,

    /// <summary>
    /// All members picked estimations and the estimation is finished.
    /// </summary>
    EstimationFinished,

    /// <summary>
    /// Estimation was canceled by Scrum master.
    /// </summary>
    EstimationCanceled
}
