using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Joins existing Scrum Team and initialize <see cref="PlanningPokerController"/>.
    /// </summary>
    public class JoinTeamController
    {
        private const string MemberExistsError1 = "Member or observer named";
        private const string MemberExistsError2 = "already exists in the team.";

        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IPlanningPokerInitializer _planningPokerInitializer;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IBusyIndicatorService _busyIndicatorService;
        private readonly IUriHelper _uriHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinTeamController"/> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to create Scrum Team on server.</param>
        /// <param name="planningPokerInitializer">Objects that initialize new Planning Poker game.</param>
        /// <param name="messageBoxService">Service to display message to user.</param>
        /// <param name="busyIndicatorService">Service to display that operation is in progress.</param>
        /// <param name="uriHelper">Service to navigate to specified URL.</param>
        public JoinTeamController(
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
        /// Joins existing Scrum Team and initialize Planning Poker game.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="memberName">Name of the joining member.</param>
        /// <param name="asObserver"><c>True</c> if member is joining as observer only; otherwise <c>false</c>.</param>
        /// <returns><c>True</c> if the operation was successful; otherwise <c>false</c>.</returns>
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
                    _planningPokerInitializer.InitializeTeam(team, memberName);
                    ControllerHelper.OpenPlanningPokerPage(_uriHelper, team, memberName);
                    return true;
                }
            }
            catch (PlanningPokerException ex)
            {
                var message = ex.Message;
                if (message.IndexOf(MemberExistsError1, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    message.IndexOf(MemberExistsError2, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    message = ControllerHelper.GetErrorMessage(ex);
                    message = $"{message}{Environment.NewLine}{Resources.JoinTeam_ReconnectMessage}";
                    if (await _messageBoxService.ShowMessage(message, Resources.JoinTeam_ReconnectTitle, Resources.JoinTeam_ReconnectButton))
                    {
                        return await ReconnectTeam(teamName, memberName, CancellationToken.None);
                    }
                }
                else
                {
                    message = ControllerHelper.GetErrorMessage(ex);
                    await _messageBoxService.ShowMessage(message, Resources.MessagePanel_Error);
                }
            }

            return false;
        }

        private async Task<bool> ReconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            try
            {
                ReconnectTeamResult reconnectTeamResult = null;
                using (_busyIndicatorService.Show())
                {
                    reconnectTeamResult = await _planningPokerService.ReconnectTeam(teamName, memberName, cancellationToken);
                }

                if (reconnectTeamResult != null)
                {
                    _planningPokerInitializer.InitializeTeam(reconnectTeamResult, memberName);
                    ControllerHelper.OpenPlanningPokerPage(_uriHelper, reconnectTeamResult.ScrumTeam, memberName);
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
