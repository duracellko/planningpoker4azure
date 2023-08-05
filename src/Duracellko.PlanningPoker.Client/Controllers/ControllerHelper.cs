using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Helper functions used by UI logic.
    /// </summary>
    internal static class ControllerHelper
    {
        private const string AutoConnectName = "AutoConnect";
        private const string CallbackUriName = "CallbackUri";
        private const string CallbackReferenceName = "CallbackReference";

        /// <summary>
        /// Gets collection of available estimation decks, which can be selected, when creating new team.
        /// </summary>
        public static IReadOnlyDictionary<Deck, string> EstimationDecks { get; } = new SortedDictionary<Deck, string>
        {
            { Deck.Standard, UIResources.EstimationDeck_Standard },
            { Deck.Fibonacci, UIResources.EstimationDeck_Fibonacci },
            { Deck.Rating, UIResources.EstimationDeck_Rating },
            { Deck.Tshirt, UIResources.EstimationDeck_Tshirt },
            { Deck.RockPaperScissorsLizardSpock, UIResources.EstimationDeck_RockPaperScissorsLizardSpock },
        };

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

            var result = GetErrorMessageFromErrorCode(exception);
            return result ?? exception.Message;
        }

        /// <summary>
        /// Opens PlanningPoker page with specified team name and member name.
        /// </summary>
        /// <param name="navigationManager">Helper to navigate to URL.</param>
        /// <param name="team">Scrum team name to include in URL.</param>
        /// <param name="member">Member name to include in URL.</param>
        /// <param name="callbackReference">The application callback reference to include in the URL.</param>
        public static void OpenPlanningPokerPage(
            INavigationManager navigationManager,
            ScrumTeam team,
            string member,
            ApplicationCallbackReference? callbackReference)
        {
            if (navigationManager == null)
            {
                throw new ArgumentNullException(nameof(navigationManager));
            }

            var urlEncoder = UrlEncoder.Default;
            var uri = $"PlanningPoker/{urlEncoder.Encode(team.Name)}/{urlEncoder.Encode(member)}";

            if (callbackReference != null)
            {
                uri += $"?{CallbackUriName}={urlEncoder.Encode(callbackReference.Url.ToString())}&{CallbackReferenceName}={urlEncoder.Encode(callbackReference.Reference)}";
            }

            navigationManager.NavigateTo(uri);
        }

        /// <summary>
        /// Opens Index page with prefilled team name and member name.
        /// </summary>
        /// <param name="navigationManager">Helper to navigate to URL.</param>
        /// <param name="team">Scrum team name to include in URL.</param>
        /// <param name="member">Member name to include in URL.</param>
        public static void OpenIndexPage(INavigationManager navigationManager, string? team, string? member)
        {
            if (navigationManager == null)
            {
                throw new ArgumentNullException(nameof(navigationManager));
            }

            var urlEncoder = UrlEncoder.Default;
            var uri = "Index";

            if (!string.IsNullOrEmpty(team))
            {
                uri += '/' + urlEncoder.Encode(team);

                if (!string.IsNullOrEmpty(member))
                {
                    uri += '/' + urlEncoder.Encode(member);
                }
            }

            navigationManager.NavigateTo(uri);
        }

        /// <summary>
        /// Parses <see cref="AutoConnectRequest"/> object from the URI.
        /// </summary>
        /// <param name="uri">The URI to parse the auto connect request and callback reference from.</param>
        /// <returns>The AutoConnectRequest object obtained from URI or null, when URI does not contain auto connect data.</returns>
        public static AutoConnectRequest? GetAutoConnectRequestFromUri(string uri)
        {
            if (!string.IsNullOrEmpty(uri) && Uri.TryCreate(uri, UriKind.Absolute, out var resultUri))
            {
                return GetAutoConnectRequestFromQueryString(resultUri.Query);
            }

            return null;
        }

        private static string? GetErrorMessageFromErrorCode(PlanningPokerException exception)
        {
            if (string.IsNullOrEmpty(exception.Error))
            {
                return null;
            }

            string? message = exception.Error switch
            {
                ErrorCodes.ScrumTeamNotExist => UIResources.Error_ScrumTeamNotExist,
                ErrorCodes.ScrumTeamAlreadyExists => UIResources.Error_ScrumTeamAlreadyExists,
                ErrorCodes.MemberAlreadyExists => UIResources.Error_MemberAlreadyExists,
                _ => null
            };

            if (message == null)
            {
                return null;
            }

            return string.Format(CultureInfo.CurrentCulture, message, exception.Argument);
        }

        private static AutoConnectRequest? GetAutoConnectRequestFromQueryString(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            bool autoConnect = false;
            string? callbackUrl = null;
            string? callbackReference = null;

            var token = new StringBuilder();
            string? name = null;
            for (int i = 0; i < query.Length; i++)
            {
                char c = query[i];
                switch (c)
                {
                    case '&':
                        ProcessValueToken(token.ToString());
                        token.Clear();
                        break;
                    case '=':
                        name = Uri.UnescapeDataString(token.ToString());
                        token.Clear();
                        break;
                    case '?':
                        if (i > 0)
                        {
                            token.Append(c);
                        }

                        break;
                    default:
                        token.Append(c);
                        break;
                }
            }

            ProcessValueToken(token.ToString());

            if (!string.IsNullOrEmpty(callbackUrl) &&
                !string.IsNullOrEmpty(callbackReference) &&
                Uri.TryCreate(callbackUrl, UriKind.Absolute, out var callbackUri))
            {
                return new AutoConnectRequest(autoConnect, new ApplicationCallbackReference(callbackUri, callbackReference));
            }

            return null;

            void ProcessValueToken(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = Uri.UnescapeDataString(value);

                    if (string.Equals(name, AutoConnectName, StringComparison.OrdinalIgnoreCase))
                    {
                        autoConnect = bool.TryParse(value, out var boolValue) && boolValue;
                    }
                    else if (string.Equals(name, CallbackUriName, StringComparison.OrdinalIgnoreCase))
                    {
                        callbackUrl = value;
                    }
                    else if (string.Equals(name, CallbackReferenceName, StringComparison.OrdinalIgnoreCase))
                    {
                        callbackReference = value;
                    }
                }
            }
        }
    }
}
