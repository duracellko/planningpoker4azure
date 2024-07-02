using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Storage of settings for timer functionality. The settings are stored in browser local storage.
/// </summary>
public class TimerSettingsRepository : ITimerSettingsRepository
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerSettingsRepository"/> class.
    /// </summary>
    /// <param name="jsRuntime">JavaScript runtime to execute JavaScript functions.</param>
    public TimerSettingsRepository(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    /// <summary>
    /// Loads duration of timer from the store.
    /// </summary>
    /// <returns>Loaded duration of timer.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore exception. Default value is used.")]
    public async Task<TimeSpan?> GetTimerDurationAsync()
    {
        try
        {
            var timerDurationString = await _jsRuntime.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration");
            if (!string.IsNullOrEmpty(timerDurationString))
            {
                return TimeSpan.ParseExact(timerDurationString, "c", CultureInfo.InvariantCulture);
            }

            return null;
        }
        catch (Exception)
        {
            // Ignore exception. Default value is used.
            return null;
        }
    }

    /// <summary>
    /// Saves duration of timer to the store.
    /// </summary>
    /// <param name="timerDuration">The duration of timer to be saved.</param>
    /// <returns>Asynchronous operation.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore exception. Default value is used, when loading.")]
    public async Task SetTimerDurationAsync(TimeSpan timerDuration)
    {
        try
        {
            var timerDurationString = timerDuration.ToString("c", CultureInfo.InvariantCulture);
            await _jsRuntime.InvokeVoidAsync("Duracellko.PlanningPoker.setTimerDuration", timerDurationString);
        }
        catch (Exception)
        {
            // Ignore exception. Default value is used, when loading.
        }
    }
}
