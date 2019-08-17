using System;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Duracellko.PlanningPoker.Web.Model
{
    public class HomeModel : PageModel
    {
        public HomeModel(PlanningPokerClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
        }

        public PlanningPokerClientConfiguration ClientConfiguration { get; }
    }
}