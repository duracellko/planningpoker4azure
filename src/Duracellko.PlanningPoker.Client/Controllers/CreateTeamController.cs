using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class CreateTeamController
    {
        private readonly PlanningPokerController _planningPokerController;
        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IBusyIndicatorService _busyIndicatorService;
        private readonly IUriHelper _uriHelper;

        public CreateTeamController(
            PlanningPokerController planningPokerController,
            IPlanningPokerClient planningPokerService,
            IMessageBoxService messageBoxService,
            IBusyIndicatorService busyIndicatorService,
            IUriHelper uriHelper)
        {
            _planningPokerController = planningPokerController ?? throw new ArgumentNullException(nameof(planningPokerController));
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _busyIndicatorService = busyIndicatorService ?? throw new ArgumentNullException(nameof(busyIndicatorService));
            _uriHelper = uriHelper ?? throw new ArgumentNullException(nameof(uriHelper));
        }

        public async Task<bool> CreateTeam(string teamName, string scrumMasterName)
        {
            if (string.IsNullOrEmpty(teamName) || string.IsNullOrEmpty(scrumMasterName))
            {
                return false;
            }

            try
            {
                ScrumTeam team = null;
                using (_busyIndicatorService.Show())
                {
                    team = await _planningPokerService.CreateTeam(teamName, scrumMasterName, CancellationToken.None);
                }

                if (team != null)
                {
                    _planningPokerController.InitializeTeam(team);
                    _uriHelper.NavigateTo("PlanningPoker");
                    return true;
                }
            }
            catch (PlanningPokerException ex)
            {
                var message = ControllerHelper.GetErrorMessage(ex);
                await _messageBoxService.ShowMessage(message, Resources.MessagePanel_Error);
            }

            return false;
        }
    }
}
