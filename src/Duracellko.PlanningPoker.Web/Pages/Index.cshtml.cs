using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Duracellko.PlanningPoker.Web.Pages
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name must match first type name", Justification = "File is associated with Index page.")]
    public class IndexModel : PageModel
    {
        public IndexModel(PlanningPokerClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
        }

        public PlanningPokerClientConfiguration ClientConfiguration { get; }
    }
}