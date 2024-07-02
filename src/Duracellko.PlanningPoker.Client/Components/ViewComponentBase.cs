using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.AspNetCore.Components;

namespace Duracellko.PlanningPoker.Client.Components;

/// <summary>
/// Base component for application components.
/// </summary>
public class ViewComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets message box service that is ised to display error to user.
    /// </summary>
    [Inject]
    public IMessageBoxService? MessageBox { get; set; }

    /// <summary>
    /// Executes specified action and, when an exception occures, it is displayed to user.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is displayed to user.")]
    protected async Task TryRun(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await ShowError(ex);
        }
    }

    /// <summary>
    /// Displays exception text to user.
    /// </summary>
    /// <param name="exception">Exception with error to display.</param>
    /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
    protected async Task ShowError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (MessageBox != null)
        {
            await MessageBox.ShowMessage(exception.Message, UIResources.MessagePanel_Error);
        }
    }
}
