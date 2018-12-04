using System;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.UI
{
    /// <summary>
    /// MessageBox service provides functionality to display message to a user.
    /// </summary>
    public class MessageBoxService : IMessageBoxService
    {
        private Func<string, string, Task> _messageHandler;

        /// <summary>
        /// Displays message to user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of message panel.</param>
        /// <returns>Task that is completed, when user confirms the message.</returns>
        public Task ShowMessage(string message, string title)
        {
            var handler = _messageHandler;
            if (handler != null)
            {
                return handler(message, title);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Displays message to user with primary button. User can click primary button to confirm action.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of message panel.</param>
        /// <param name="primaryButton">Text displayed on primary button used to confirm action.</param>
        /// <returns><c>True</c> if user clicked the primary button; otherwise <c>false</c>.</returns>
        public Task<bool> ShowMessage(string message, string title, string primaryButton)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Setup handler function that displays message box dialog in Blazor component.
        /// </summary>
        /// <param name="handler">Handler delegate to display message box.</param>
        public void SetMessageHandler(Func<string, string, Task> handler)
        {
            _messageHandler = handler;
        }
    }
}
