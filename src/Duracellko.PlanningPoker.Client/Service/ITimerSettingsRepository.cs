using System;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Storage of settings for timer functionality.
    /// </summary>
    public interface ITimerSettingsRepository
    {
        /// <summary>
        /// Loads duration of timer from the store.
        /// </summary>
        /// <returns>Loaded duration of timer.</returns>
        Task<TimeSpan?> GetTimerDurationAsync();

        /// <summary>
        /// Saves duration of timer to the store.
        /// </summary>
        /// <param name="timerDuration">The duration of timer to be saved.</param>
        /// <returns>Asynchronous operation.</returns>
        Task SetTimerDurationAsync(TimeSpan timerDuration);
    }
}
