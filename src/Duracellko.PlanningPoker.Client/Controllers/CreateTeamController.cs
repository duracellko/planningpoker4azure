using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Creates new Scrum Team and initialize <see cref="PlanningPokerController"/>.
    /// </summary>
    public class CreateTeamController
    {
        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IPlanningPokerInitializer _planningPokerInitializer;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IBusyIndicatorService _busyIndicatorService;
        private readonly IUriHelper _uriHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTeamController"/> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to create Scrum Team on server.</param>
        /// <param name="planningPokerInitializer">Objects that initialize new Planning Poker game.</param>
        /// <param name="messageBoxService">Service to display message to user.</param>
        /// <param name="busyIndicatorService">Service to display that operation is in progress.</param>
        /// <param name="uriHelper">Service to navigate to specified URL.</param>
        public CreateTeamController(
            IPlanningPokerClient planningPokerService,
            IPlanningPokerInitializer planningPokerInitializer,
            IMessageBoxService messageBoxService,
            IBusyIndicatorService busyIndicatorService,
            IUriHelper uriHelper)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _planningPokerInitializer = planningPokerInitializer ?? throw new ArgumentNullException(nameof(planningPokerInitializer));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _busyIndicatorService = busyIndicatorService ?? throw new ArgumentNullException(nameof(busyIndicatorService));
            _uriHelper = uriHelper ?? throw new ArgumentNullException(nameof(uriHelper));
        }

        /// <summary>
        /// Creates new Scrum Team and initialize Planning Poker game.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of Scrum Master.</param>
        /// <returns><c>True</c> if the operation was successful; otherwise <c>false</c>.</returns>
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
                    _planningPokerInitializer.InitializeTeam(team, scrumMasterName);
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
