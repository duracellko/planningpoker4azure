using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;

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
        private readonly INavigationManager _navigationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTeamController"/> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to create Scrum Team on server.</param>
        /// <param name="planningPokerInitializer">Objects that initialize new Planning Poker game.</param>
        /// <param name="messageBoxService">Service to display message to user.</param>
        /// <param name="busyIndicatorService">Service to display that operation is in progress.</param>
        /// <param name="navigationManager">Service to navigate to specified URL.</param>
        public CreateTeamController(
            IPlanningPokerClient planningPokerService,
            IPlanningPokerInitializer planningPokerInitializer,
            IMessageBoxService messageBoxService,
            IBusyIndicatorService busyIndicatorService,
            INavigationManager navigationManager)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _planningPokerInitializer = planningPokerInitializer ?? throw new ArgumentNullException(nameof(planningPokerInitializer));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _busyIndicatorService = busyIndicatorService ?? throw new ArgumentNullException(nameof(busyIndicatorService));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <summary>
        /// Gets collection of available estimation decks, which can be selected, when creating new team.
        /// </summary>
        public IDictionary<Deck, string> EstimationDecks { get; } = new SortedDictionary<Deck, string>
        {
            { Deck.Standard, Resources.EstimationDeck_Standard },
            { Deck.Fibonacci, Resources.EstimationDeck_Fibonacci },
        };

        /// <summary>
        /// Creates new Scrum Team and initialize Planning Poker game.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of Scrum Master.</param>
        /// <param name="deck">Selected deck of estimation cards to use in the team.</param>
        /// <returns><c>True</c> if the operation was successful; otherwise <c>false</c>.</returns>
        public async Task<bool> CreateTeam(string teamName, string scrumMasterName, Deck deck)
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
                    team = await _planningPokerService.CreateTeam(teamName, scrumMasterName, deck, CancellationToken.None);
                }

                if (team != null)
                {
                    await _planningPokerInitializer.InitializeTeam(team, scrumMasterName);
                    ControllerHelper.OpenPlanningPokerPage(_navigationManager, team, scrumMasterName);
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
