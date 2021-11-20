using System;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Factory object that can start periodic invocation of specific action.
    /// </summary>
    public interface ITimerFactory
    {
        /// <summary>
        /// Starts periodic invocation of specified action.
        /// </summary>
        /// <param name="action">The action to invoke periodically.</param>
        /// <returns>The disposable object that should be disposed to stop the timer.</returns>
        IDisposable StartTimer(Action action);
    }
}
