using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;

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
        private readonly INavigationManager _navigationManager;
        private readonly IMemberCredentialsStore _memberCredentialsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinTeamController"/> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to create Scrum Team on server.</param>
        /// <param name="planningPokerInitializer">Objects that initialize new Planning Poker game.</param>
        /// <param name="messageBoxService">Service to display message to user.</param>
        /// <param name="busyIndicatorService">Service to display that operation is in progress.</param>
        /// <param name="navigationManager">Service to navigate to specified URL.</param>
        /// <param name="memberCredentialsStore">Service to save and load member credentials.</param>
        public JoinTeamController(
            IPlanningPokerClient planningPokerService,
            IPlanningPokerInitializer planningPokerInitializer,
            IMessageBoxService messageBoxService,
            IBusyIndicatorService busyIndicatorService,
            INavigationManager navigationManager,
            IMemberCredentialsStore memberCredentialsStore)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _planningPokerInitializer = planningPokerInitializer ?? throw new ArgumentNullException(nameof(planningPokerInitializer));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _busyIndicatorService = busyIndicatorService ?? throw new ArgumentNullException(nameof(busyIndicatorService));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            _memberCredentialsStore = memberCredentialsStore ?? throw new ArgumentNullException(nameof(memberCredentialsStore));
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
                TeamResult teamResult = null;
                using (_busyIndicatorService.Show())
                {
                    teamResult = await _planningPokerService.JoinTeam(teamName, memberName, asObserver, CancellationToken.None);
                }

                if (teamResult != null)
                {
                    await _planningPokerInitializer.InitializeTeam(teamResult, memberName);
                    ControllerHelper.OpenPlanningPokerPage(_navigationManager, teamResult.ScrumTeam, memberName);
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
                        return await ReconnectTeam(teamName, memberName, false, CancellationToken.None);
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

        /// <summary>
        /// Reconnects member to team, when credentials are stored.
        /// </summary>
        /// <param name="teamName">Name of team to reconnect to.</param>
        /// <param name="memberName">Name of member to reconnect.</param>
        /// <returns><c>True</c> if the operation was successful; otherwise <c>false</c>.</returns>
        public async Task<bool> TryReconnectTeam(string teamName, string memberName)
        {
            if (string.IsNullOrEmpty(teamName) || string.IsNullOrEmpty(memberName))
            {
                return false;
            }

            var memberCredentials = await _memberCredentialsStore.GetCredentialsAsync();
            if (memberCredentials != null &&
                string.Equals(memberCredentials.TeamName, teamName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(memberCredentials.MemberName, memberName, StringComparison.OrdinalIgnoreCase))
            {
                return await ReconnectTeam(teamName, memberName, true, CancellationToken.None);
            }

            return false;
        }

        private async Task<bool> ReconnectTeam(string teamName, string memberName, bool ignoreError, CancellationToken cancellationToken)
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
                    await _planningPokerInitializer.InitializeTeam(reconnectTeamResult, memberName);
                    ControllerHelper.OpenPlanningPokerPage(_navigationManager, reconnectTeamResult.ScrumTeam, memberName);
                    return true;
                }
            }
            catch (PlanningPokerException ex)
            {
                if (!ignoreError)
                {
                    var message = ControllerHelper.GetErrorMessage(ex);
                    await _messageBoxService.ShowMessage(message, Resources.MessagePanel_Error);
                }
            }

            return false;
        }
    }
}
