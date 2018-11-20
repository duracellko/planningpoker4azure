using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class JoinTeamController
    {
        private readonly PlanningPokerController _planningPokerController;
        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IBusyIndicatorService _busyIndicatorService;
        private readonly IUriHelper _uriHelper;

        public JoinTeamController(
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

        public async Task<bool> JoinTeam(string teamName, string memberName, bool asObserver)
        {
            if (string.IsNullOrEmpty(teamName) || string.IsNullOrEmpty(memberName))
            {
                return false;
            }

            try
            {
                ScrumTeam team = null;
                using (_busyIndicatorService.Show())
                {
                    team = await _planningPokerService.JoinTeam(teamName, memberName, asObserver, CancellationToken.None);
                }

                if (team != null)
                {
                    _planningPokerController.InitializeTeam(team, memberName);
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
