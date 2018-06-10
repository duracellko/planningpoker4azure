using System.Linq;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Duracellko.PlanningPoker.Web
{
    public class PlanningPokerApplication : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            // Remove PlanningPokerContainer, because it is not MVC controller.
            var planningPokerController = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(PlanningPokerController));
            if (planningPokerController != null)
            {
                application.Controllers.Remove(planningPokerController);
            }

            planningPokerController = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(AzurePlanningPokerController));
            if (planningPokerController != null)
            {
                application.Controllers.Remove(planningPokerController);
            }
        }
    }
}
