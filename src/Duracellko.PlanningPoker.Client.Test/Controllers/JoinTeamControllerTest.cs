using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class JoinTeamControllerTest
    {
        private const string ErrorMessage = "Planning Poker Error";
        private const string ReconnectErrorMessage = "Member or observer named 'Test member' already exists in the team.";

        [DataTestMethod]
        [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
        [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
        public async Task JoinTeam_MemberName_JoinTeamOnService(string memberName, bool asObserver)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(scrumTeam);
            var target = CreateController(planningPokerService: planningPokerService.Object);

            await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

            planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver, It.IsAny<CancellationToken>()));
            planningPokerService.Verify(o => o.ReconnectTeam(PlanningPokerData.TeamName, memberName, It.IsAny<CancellationToken>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
        [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
        public async Task JoinTeam_MemberName_ReturnsTrue(string memberName, bool asObserver)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(scrumTeam: scrumTeam);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

            Assert.IsTrue(result);
        }

        [DataTestMethod]
        [DataRow("", PlanningPokerData.MemberName, false, DisplayName = "Team name is Empty and not observer")]
        [DataRow(null, PlanningPokerData.MemberName, false, DisplayName = "Team name is Null and not observer")]
        [DataRow(PlanningPokerData.TeamName, "", false, DisplayName = "Member name is Empty and not observer")]
        [DataRow(PlanningPokerData.TeamName, null, false, DisplayName = "Member name is Null and not observer")]
        [DataRow("", PlanningPokerData.MemberName, true, DisplayName = "Team name is Empty and observer")]
        [DataRow(null, PlanningPokerData.MemberName, true, DisplayName = "Team name is Null and observer")]
        [DataRow(PlanningPokerData.TeamName, "", true, DisplayName = "Member name is Empty and observer")]
        [DataRow(PlanningPokerData.TeamName, null, true, DisplayName = "Member name is Null and observer")]
        public async Task JoinTeam_TeamNameOrMemberNameIsEmpty_ReturnsFalse(string teamName, string memberName, bool asObserver)
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var target = CreateController(planningPokerService: planningPokerService.Object);

            var result = await target.JoinTeam(teamName, memberName, asObserver);

            Assert.IsFalse(result);
            planningPokerService.Verify(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
            planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [DataTestMethod]
        public async Task JoinTeam_ServiceReturnsTeam_InitializePlanningPokerController()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, scrumTeam: scrumTeam);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            planningPokerInitializer.Verify(o => o.InitializeTeam(scrumTeam, PlanningPokerData.MemberName));
        }

        [TestMethod]
        public async Task JoinTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var uriHelper = new Mock<IUriHelper>();
            var target = CreateController(uriHelper: uriHelper.Object, scrumTeam: scrumTeam);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

            uriHelper.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer"));
        }

        [TestMethod]
        public async Task JoinTeam_ServiceThrowsException_ReturnsFalse()
        {
            var target = CreateController(errorMessage: ErrorMessage);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task JoinTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
        {
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();

            var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ScrumTeam>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task JoinTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
        {
            var uriHelper = new Mock<IUriHelper>();

            var target = CreateController(uriHelper: uriHelper.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            uriHelper.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task JoinTeam_ServiceThrowsException_ShowsMessage()
        {
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            messageBoxService.Verify(o => o.ShowMessage(ErrorMessage, "Error"));
        }

        [TestMethod]
        public async Task JoinTeam_ServiceThrowsException_Shows1LineMessage()
        {
            var errorMessage = "Planning Poker Error\r\nArgumentException";
            var messageBoxService = new Mock<IMessageBoxService>();

            var target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error\r", "Error"));
        }

        [TestMethod]
        public async Task JoinTeam_TeamName_ShowsBusyIndicator()
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var joinTeamTask = new TaskCompletionSource<ScrumTeam>();
            planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(joinTeamTask.Task);
            var busyIndicatorInstance = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance.Object);
            var target = CreateController(planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

            var result = target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorInstance.Verify(o => o.Dispose(), Times.Never());

            joinTeamTask.SetResult(PlanningPokerData.GetScrumTeam());
            await result;

            busyIndicatorInstance.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task JoinTeam_MemberAlreadyExistsButUserDoesNotReconnect_DoesNotReconnect()
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage));
            var messageBoxService = new Mock<IMessageBoxService>();
            SetupReconnectMessageBox(messageBoxService, false);

            var target = CreateController(planningPokerService: planningPokerService.Object, messageBoxService: messageBoxService.Object);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            var userMessage = ReconnectErrorMessage + Environment.NewLine + "Do you want to reconnect?";
            messageBoxService.Verify(o => o.ShowMessage(userMessage, "Reconnect", "Reconnect"));
            planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
        [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
        public async Task ReconnectTeam_MemberName_JoinTeamOnService(string memberName, bool asObserver)
        {
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            var planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage));
            planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reconnectTeamResult);
            var target = CreateController(memberExistsError: true, planningPokerService: planningPokerService.Object);

            await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

            planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver, It.IsAny<CancellationToken>()));
            planningPokerService.Verify(o => o.ReconnectTeam(PlanningPokerData.TeamName, memberName, It.IsAny<CancellationToken>()));
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
        [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
        public async Task ReconnectTeam_MemberName_ReturnsTrue(string memberName, bool asObserver)
        {
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            var target = CreateController(memberExistsError: true, reconnectTeamResult: reconnectTeamResult);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

            Assert.IsTrue(result);
        }

        [DataTestMethod]
        public async Task ReconnectTeam_ServiceReturnsTeam_InitializePlanningPokerController()
        {
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var target = CreateController(memberExistsError: true, planningPokerInitializer: planningPokerInitializer.Object, reconnectTeamResult: reconnectTeamResult);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName));
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
        {
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            var uriHelper = new Mock<IUriHelper>();
            var target = CreateController(memberExistsError: true, uriHelper: uriHelper.Object, reconnectTeamResult: reconnectTeamResult);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

            uriHelper.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer"));
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceThrowsException_ReturnsFalse()
        {
            var target = CreateController(memberExistsError: true, errorMessage: ErrorMessage);

            var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
        {
            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();

            var target = CreateController(memberExistsError: true, planningPokerInitializer: planningPokerInitializer.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ReconnectTeamResult>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
        {
            var uriHelper = new Mock<IUriHelper>();

            var target = CreateController(memberExistsError: true, uriHelper: uriHelper.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            uriHelper.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceThrowsException_ShowsMessage()
        {
            var messageBoxService = new Mock<IMessageBoxService>();
            SetupReconnectMessageBox(messageBoxService, true);

            var target = CreateController(memberExistsError: true, messageBoxService: messageBoxService.Object, errorMessage: ErrorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            messageBoxService.Verify(o => o.ShowMessage(ErrorMessage, "Error"));
        }

        [TestMethod]
        public async Task ReconnectTeam_ServiceThrowsException_Shows1LineMessage()
        {
            var errorMessage = "Planning Poker Error\r\nArgumentException";
            var messageBoxService = new Mock<IMessageBoxService>();
            SetupReconnectMessageBox(messageBoxService, true);

            var target = CreateController(memberExistsError: true, messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error\r", "Error"));
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamName_ShowsBusyIndicator()
        {
            var planningPokerService = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var joinTeamTask = new TaskCompletionSource<ScrumTeam>();
            var reconnectTeamTask = new TaskCompletionSource<ReconnectTeamResult>();
            planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(joinTeamTask.Task);
            planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(reconnectTeamTask.Task);
            var target = CreateController(memberExistsError: true, planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

            var busyIndicatorInstance1 = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance1.Object);

            var result = target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorInstance1.Verify(o => o.Dispose(), Times.Never());

            var busyIndicatorInstance2 = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance2.Object);

            joinTeamTask.SetException(new PlanningPokerException(ReconnectErrorMessage));

            busyIndicatorService.Verify(o => o.Show(), Times.Exactly(2));
            busyIndicatorInstance1.Verify(o => o.Dispose());
            busyIndicatorInstance2.Verify(o => o.Dispose(), Times.Never());

            reconnectTeamTask.SetResult(PlanningPokerData.GetReconnectTeamResult());
            await result;

            busyIndicatorInstance2.Verify(o => o.Dispose());
        }

        private static JoinTeamController CreateController(
            IPlanningPokerInitializer planningPokerInitializer = null,
            IPlanningPokerClient planningPokerService = null,
            IMessageBoxService messageBoxService = null,
            IBusyIndicatorService busyIndicatorService = null,
            IUriHelper uriHelper = null,
            bool memberExistsError = false,
            ScrumTeam scrumTeam = null,
            ReconnectTeamResult reconnectTeamResult = null,
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
                var joinSetup = planningPokerServiceMock.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
                var reconnectSetup = planningPokerServiceMock.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
                if (memberExistsError)
                {
                    joinSetup.ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage));
                    if (errorMessage == null)
                    {
                        reconnectSetup.ReturnsAsync(reconnectTeamResult);
                    }
                    else
                    {
                        reconnectSetup.ThrowsAsync(new PlanningPokerException(errorMessage));
                    }
                }
                else
                {
                    if (errorMessage == null)
                    {
                        joinSetup.ReturnsAsync(scrumTeam);
                    }
                    else
                    {
                        joinSetup.ThrowsAsync(new PlanningPokerException(errorMessage));
                    }
                }

                planningPokerService = planningPokerServiceMock.Object;
            }

            if (messageBoxService == null)
            {
                var messageBoxServiceMock = new Mock<IMessageBoxService>();
                if (memberExistsError)
                {
                    SetupReconnectMessageBox(messageBoxServiceMock, true);
                }

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

            return new JoinTeamController(planningPokerService, planningPokerInitializer, messageBoxService, busyIndicatorService, uriHelper);
        }

        private static void SetupReconnectMessageBox(Mock<IMessageBoxService> messageBoxService, bool result)
        {
            messageBoxService.Setup(o => o.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(result);
        }
    }
}
