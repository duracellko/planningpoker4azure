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

        /// <summary>
        /// Displays message to user with primary button. User can click primary button to confirm action.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of message panel.</param>
        /// <param name="primaryButton">Text displayed on primary button used to confirm action.</param>
        /// <returns><c>True</c> if user clicked the primary button; otherwise <c>false</c>.</returns>
        Task<bool> ShowMessage(string message, string title, string primaryButton);
    }
}
