using System;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Helper functions used by UI logic.
    /// </summary>
    public static class ControllerHelper
    {
        /// <summary>
        /// Gets user friendly message from <see cref="PlanningPokerException"/> object.
        /// </summary>
        /// <param name="exception">Exception to get message for.</param>
        /// <returns>User friendly text message.</returns>
        public static string GetErrorMessage(PlanningPokerException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var result = exception.Message;
            var newLineIndex = result.IndexOf('\n');
            if (newLineIndex >= 0)
            {
                result = result.Substring(0, newLineIndex);
            }

            return result;
        }
    }
}
