using System;
using System.Text.Json;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Converts estimation values on Scrum Team entities.
/// </summary>
/// <remarks>
/// Infinity value cannot be serialized in JSON, so it is mapped from numeric value.
/// </remarks>
internal static class ScrumTeamMapper
{
    /// <summary>
    /// Gets a new <see cref="PlanningPokerException"/> from response value.
    /// </summary>
    /// <param name="value">The response value that contains data for the PlanningPokerException.</param>
    /// <returns>A new instance of PlanningPokerException.</returns>
    public static PlanningPokerException GetPlanningPokerException(string? value) => GetPlanningPokerException(value, null);

    /// <summary>
    /// Gets a new <see cref="PlanningPokerException"/> from response value.
    /// </summary>
    /// <param name="value">The response value that contains data for the PlanningPokerException.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <returns>A new instance of PlanningPokerException.</returns>
    public static PlanningPokerException GetPlanningPokerException(string? value, Exception? innerException)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new PlanningPokerException(value, innerException);
        }

        var prefix = nameof(PlanningPokerException) + ':';
        var index = value.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var dataJson = value[(index + prefix.Length)..];
            var data = JsonSerializer.Deserialize<PlanningPokerExceptionData>(dataJson);
            if (data != null)
            {
                return new PlanningPokerException(data.Message, data.Error, data.Argument, innerException);
            }
        }

        return new PlanningPokerException(value, innerException);
    }
}
