using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class PlanningPokerController
    {
        private readonly IPlanningPokerClient _planningPokerService;

        public PlanningPokerController(IPlanningPokerClient planningPokerService)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
        }

        public ScrumTeam ScrumTeam { get; private set; }

        public void InitializeTeam(ScrumTeam scrumTeam)
        {
            ScrumTeam = scrumTeam;
        }
    }
}
