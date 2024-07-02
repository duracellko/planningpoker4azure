using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker;

/// <summary>
/// Object provides system tasks, e.g. delay task.
/// </summary>
public class TaskProvider
{
    /// <summary>
    /// Gets default instance of task provider.
    /// </summary>
    public static TaskProvider Default { get; } = new TaskProvider();

    /// <summary>
    /// Creates a cancellable task that completes after a specified time interval.
    /// </summary>
    /// <param name="delay">The time span to wait before completing the returned task, or TimeSpan.FromMilliseconds(-1) to wait indefinitely.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the time delay.</returns>
    public virtual Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
