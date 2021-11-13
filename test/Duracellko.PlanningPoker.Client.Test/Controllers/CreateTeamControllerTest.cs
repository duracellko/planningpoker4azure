using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class CreateTeamControllerTest
    {
        [TestMethod]
        public void EstimationDecks_Get_ReturnsEstimationDecks()
        {
            var target = CreateController();

            var result = target.EstimationDecks;

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100", result[Deck.Standard]);
            Assert.AreEqual("0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89", result[Deck.Fibonacci]);
            Assert.AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9, 10", result[Deck.Rating]);
            Assert.AreEqual("T-shirt: XS, S, M, L, XL", result[Deck.Tshirt]);
            Assert.AreEqual("Rock, Paper, Scissors, Lizard, Spock", result[Deck.RockPaperScissorsLizardSpock]);
        }

        [DataTestMethod]
        [DataRow(Deck.Standard)]
        [DataRow(Deck.Fibonacci)]
        [DataRow(Deck.RockPaperScissorsLizardSpock)]
        public async Task CreateTeam_TeamName_CreateTeamOnService(Deck deck)
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
            var planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Deck>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(teamResult);
            var target = CreateController(planningPokerService: planningPokerService.Object);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, deck);

            planningPokerService.Verify(o => o.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, deck, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameAndScrumMasterName_ReturnTrue()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
            var target = CreateController(teamResult: teamResult);

            var result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            Assert.IsTrue(result);
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.TeamName, "", DisplayName = "ScrumMasterName Is Empty")]
        [DataRow(PlanningPokerData.TeamName, null, DisplayName = "ScrumMasterName Is Null")]
        [DataRow("", PlanningPokerData.ScrumMasterName, DisplayName = "TeamName Is Empty")]
        [DataRow(null, PlanningPokerData.ScrumMasterName, DisplayName = "TeamName Is Null")]
        public async Task CreateTeam_TeamNameOrScrumMasterNameIsEmpty_ReturnFalse(string teamName, string scrumMasterName)
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var target = CreateController(planningPokerService: planningPokerService.Object);

            var result = await target.CreateTeam(teamName, scrumMasterName, Deck.Standard);

            Assert.IsFalse(result);
            planningPokerService.Verify(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Deck>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_InitializePlanningPokerController()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, teamResult: teamResult);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            planningPokerInitializer.Verify(o => o.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
            var navigationManager = new Mock<INavigationManager>();
            var target = CreateController(navigationManager: navigationManager.Object, teamResult: teamResult);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20Scrum%20Master"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ReturnsFalse()
        {
            var target = CreateController(errorMessage: string.Empty);

            var result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
        {
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();

            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
        {
            var navigationManager = new Mock<INavigationManager>();

            var target = CreateController(navigationManager: navigationManager.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ShowsMessage()
        {
            var errorMessage = "Planning Poker Error";
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_Shows1LineMessage()
        {
            var errorMessage = "Planning Poker Error\r\nArgumentException";
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error\r", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_TeamName_ShowsBusyIndicator()
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var createTeamTask = new TaskCompletionSource<TeamResult>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Deck>(), It.IsAny<CancellationToken>()))
                .Returns(createTeamTask.Task);
            var busyIndicatorInstance = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance.Object);
            var target = CreateController(planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

            var result = target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorInstance.Verify(o => o.Dispose(), Times.Never());

            createTeamTask.SetResult(PlanningPokerData.GetTeamResult(PlanningPokerData.GetInitialScrumTeam()));
            await result;

            busyIndicatorInstance.Verify(o => o.Dispose());
        }

        private static CreateTeamController CreateController(
            IPlanningPokerInitializer? planningPokerInitializer = null,
            IPlanningPokerClient? planningPokerService = null,
            IMessageBoxService? messageBoxService = null,
            IBusyIndicatorService? busyIndicatorService = null,
            INavigationManager? navigationManager = null,
            TeamResult? teamResult = null,
            string? errorMessage = null)
        {
            if (planningPokerInitializer == null)
            {
                var planningPokerInitializerMock = new Mock<IPlanningPokerInitializer>();
                planningPokerInitializer = planningPokerInitializerMock.Object;
            }

            if (planningPokerService == null)
            {
                var planningPokerServiceMock = new Mock<IPlanningPokerClient>();
                var createSetup = planningPokerServiceMock.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Deck>(), It.IsAny<CancellationToken>()));
                if (errorMessage == null)
                {
                    createSetup!.ReturnsAsync(teamResult);
                }
                else
                {
                    createSetup.ThrowsAsync(new PlanningPokerException(errorMessage));
                }

                planningPokerService = planningPokerServiceMock.Object;
            }

            if (messageBoxService == null)
            {
                var messageBoxServiceMock = new Mock<IMessageBoxService>();
                messageBoxService = messageBoxServiceMock.Object;
            }

            if (busyIndicatorService == null)
            {
                var busyIndicatorServiceMock = new Mock<IBusyIndicatorService>();
                busyIndicatorService = busyIndicatorServiceMock.Object;
            }

            if (navigationManager == null)
            {
                var navigationManagerMock = new Mock<INavigationManager>();
                navigationManager = navigationManagerMock.Object;
            }

            return new CreateTeamController(planningPokerService, planningPokerInitializer, messageBoxService, busyIndicatorService, navigationManager);
        }
    }
}
