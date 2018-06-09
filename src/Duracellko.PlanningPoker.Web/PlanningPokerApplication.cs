using System.Linq;
using Duracellko.PlanningPoker.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Duracellko.PlanningPoker.Web
{
    public class PlanningPokerApplication : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            // Remove PlanningPokerContainer, because if is not MVC controller.
            var planningPokerController = application.Controllers.FirstOrDefault(c => c.ControllerName == nameof(PlanningPokerController));
            if (planningPokerController != null)
            {
                application.Controllers.Remove(planningPokerController);
            }
        }
    }
}
