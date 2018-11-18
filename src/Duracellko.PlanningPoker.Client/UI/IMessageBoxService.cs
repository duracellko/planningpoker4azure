using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.UI
{
    /// <summary>
    /// MessageBox service provides functionality to display message to a user.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Displays message to user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of message panel.</param>
        /// <returns>Task that is completed, when user confirms the message.</returns>
        Task ShowMessage(string message, string title);
    }
}
