using System;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// The request that specifies, whether a user should automatically join the team or not.
/// Data for request are parsed from URL and are usually requested from an external application.
/// </summary>
public class AutoConnectRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoConnectRequest"/> class.
    /// </summary>
    /// <param name="joinAutomatically">The value indicating whether a user should automatically join the team.</param>
    /// <param name="callbackReference">The reference data to call back the application that started the Planning Poker after estimation is finished.</param>
    public AutoConnectRequest(bool joinAutomatically, ApplicationCallbackReference callbackReference)
    {
        JoinAutomatically = joinAutomatically;
        CallbackReference = callbackReference ?? throw new ArgumentNullException(nameof(callbackReference));
    }

    /// <summary>
    /// Gets a value indicating whether a user should automatically join the team.
    /// </summary>
    public bool JoinAutomatically { get; }

    /// <summary>
    /// Gets a reference data to call back the application that started the Planning Poker after estimation is finished.
    /// </summary>
    public ApplicationCallbackReference CallbackReference { get; }
}
