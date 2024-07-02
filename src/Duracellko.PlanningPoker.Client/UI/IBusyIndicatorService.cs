using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.UI;

/// <summary>
/// Object provides functionality to display busy indicator during long running operation.
/// </summary>
public interface IBusyIndicatorService
{
    /// <summary>
    /// Displays busy indicator to notify user about running operation.
    /// </summary>
    /// <returns><see cref="IDisposable"/> object that should be disposed, when operation is finished.</returns>
    IDisposable Show();
}
