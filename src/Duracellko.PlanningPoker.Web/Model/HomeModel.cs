using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;

namespace Duracellko.PlanningPoker.Web.Model
{
    public class HomeModel : PageModel
    {
        // Regular Expression pattern to match mobile User Agent.
        // Source: http://detectmobilebrowsers.com/
        private const string MobileUserAgentPattern = @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino";

        public HomeModel(PlanningPokerClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
        }

        public PlanningPokerClientConfiguration ClientConfiguration { get; }

        public bool UseServerSide
        {
            get
            {
                var useServerSide = ClientConfiguration.UseServerSide;
                if (useServerSide == ServerSideConditions.Never)
                {
                    return false;
                }
                else if (useServerSide == ServerSideConditions.Always)
                {
                    return true;
                }
                else if ((useServerSide & ServerSideConditions.Mobile) == ServerSideConditions.Mobile && !IsMobileBrowser)
                {
                    return false;
                }

                return true;
            }
        }

        private bool IsMobileBrowser
        {
            get
            {
                var userAgent = Request.Headers[HeaderNames.UserAgent].ToString();
                if (string.IsNullOrEmpty(userAgent))
                {
                    return false;
                }

                try
                {
                    var timeout = TimeSpan.FromMilliseconds(200);
                    return Regex.IsMatch(userAgent, MobileUserAgentPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline, timeout);
                }
                catch (TimeoutException)
                {
                    // When User Agent is too complicated, then run Blazor on client-side.
                    return false;
                }
            }
        }
    }
}