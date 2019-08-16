using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class CreateTeamControllerTest
    {
        [TestMethod]
        public async Task CreateTeam_TeamName_CreateTeamOnService()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(scrumTeam);
            var target = CreateController(planningPokerService: planningPokerService.Object);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerService.Verify(o => o.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameAndScrumMasterName_ReturnTrue()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var target = CreateController(scrumTeam: scrumTeam);

            var result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

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

            var result = await target.CreateTeam(teamName, scrumMasterName);

            Assert.IsFalse(result);
            planningPokerService.Verify(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_InitializePlanningPokerController()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, scrumTeam: scrumTeam);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerInitializer.Verify(o => o.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
        {
            var scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            var uriHelper = new Mock<IUriHelper>();
            var target = CreateController(uriHelper: uriHelper.Object, scrumTeam: scrumTeam);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            uriHelper.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20Scrum%20Master"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ReturnsFalse()
        {
            var target = CreateController(errorMessage: string.Empty);

            var result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
        {
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();

            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ScrumTeam>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
        {
            var uriHelper = new Mock<IUriHelper>();

            var target = CreateController(uriHelper: uriHelper.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            uriHelper.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ShowsMessage()
        {
            var errorMessage = "Planning Poker Error";
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_Shows1LineMessage()
        {
            var errorMessage = "Planning Poker Error\r\nArgumentException";
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error\r", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_TeamName_ShowsBusyIndicator()
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var createTeamTask = new TaskCompletionSource<ScrumTeam>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(createTeamTask.Task);
            var busyIndicatorInstance = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance.Object);
            var target = CreateController(planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

            var result = target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorInstance.Verify(o => o.Dispose(), Times.Never());

            createTeamTask.SetResult(PlanningPokerData.GetInitialScrumTeam());
            await result;

            busyIndicatorInstance.Verify(o => o.Dispose());
        }

        private static CreateTeamController CreateController(
            IPlanningPokerInitializer planningPokerInitializer = null,
            IPlanningPokerClient planningPokerService = null,
            IMessageBoxService messageBoxService = null,
            IBusyIndicatorService busyIndicatorService = null,
            IUriHelper uriHelper = null,
            ScrumTeam scrumTeam = null,
            string errorMessage = null)
        {
            if (planningPokerInitializer == null)
            {
                var planningPokerInitializerMock = new Mock<IPlanningPokerInitializer>();
                planningPokerInitializer = planningPokerInitializerMock.Object;
            }

            if (planningPokerService == null)
            {
                var planningPokerServiceMock = new Mock<IPlanningPokerClient>();
                var createSetup = planningPokerServiceMock.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
                if (errorMessage == null)
                {
                    createSetup.ReturnsAsync(scrumTeam);
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

            if (uriHelper == null)
            {
                var uriHelperMock = new Mock<IUriHelper>();
                uriHelper = uriHelperMock.Object;
            }

            return new CreateTeamController(planningPokerService, planningPokerInitializer, messageBoxService, busyIndicatorService, uriHelper);
        }
    }
}
