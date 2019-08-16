using System;
using System.Text.Encodings.Web;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Components;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Helper functions used by UI logic.
    /// </summary>
    internal static class ControllerHelper
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

        /// <summary>
        /// Opens PlanningPoker page with specified team name and member name.
        /// </summary>
        /// <param name="uriHelper">Helper to navigate to URL.</param>
        /// <param name="team">Scrum team name to include in URL.</param>
        /// <param name="member">Member name to include in URL.</param>
        public static void OpenPlanningPokerPage(IUriHelper uriHelper, ScrumTeam team, string member)
        {
            if (uriHelper == null)
            {
                throw new ArgumentNullException(nameof(uriHelper));
            }

            var urlEncoder = UrlEncoder.Default;
            var uri = $"PlanningPoker/{urlEncoder.Encode(team.Name)}/{urlEncoder.Encode(member)}";
            uriHelper.NavigateTo(uri);
        }
    }
}
