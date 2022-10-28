using System;
using System.Threading;
using AngleSharp.Html.Dom;
using AngleSharpWrappers;
using Bunit;
using Duracellko.PlanningPoker.Client.Components;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [TestClass]
    public sealed class JoinTeamPanelTest : IDisposable
    {
        private Bunit.TestContext _context = new Bunit.TestContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void Initialized_NoValidationErrors()
        {
            var controller = CreateJoinTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            var memberNameElement = target.Find("input[name=memberName]");
            Assert.AreEqual("form-control", memberNameElement.ClassName);
            validationElements = memberNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetTeamNameAndMemberName_NoValidationErrors()
        {
            var controller = CreateJoinTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var memberNameElement = target.Find("input[name=memberName]");
            memberNameElement.Change(PlanningPokerData.MemberName);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            memberNameElement = target.Find("input[name=memberName]");
            Assert.AreEqual("form-control", memberNameElement.ClassName);
            validationElements = memberNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetTeamNameToEmpty_RequiredValidationError()
        {
            var controller = CreateJoinTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var memberNameElement = target.Find("input[name=memberName]");
            memberNameElement.Change(PlanningPokerData.MemberName);

            teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(string.Empty);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control is-invalid", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(1, validationElements.Length);
            Assert.AreEqual("Required", validationElements[0].TextContent);

            memberNameElement = target.Find("input[name=memberName]");
            Assert.AreEqual("form-control", memberNameElement.ClassName);
            validationElements = memberNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetMemberNameToEmpty_RequiredValidationError()
        {
            var controller = CreateJoinTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var memberNameElement = target.Find("input[name=memberName]");
            memberNameElement.Change(PlanningPokerData.MemberName);

            memberNameElement = target.Find("input[name=memberName]");
            memberNameElement.Change(string.Empty);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            memberNameElement = target.Find("input[name=memberName]");
            Assert.AreEqual("form-control is-invalid", memberNameElement.ClassName);
            validationElements = memberNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(1, validationElements.Length);
            Assert.AreEqual("Required", validationElements[0].TextContent);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ClickCreate_SendsJoinTeamRequest(bool asObserver)
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var controller = CreateJoinTeamController(planningPokerClient: planningPokerClient.Object);
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var memberNameElement = target.Find("input[name=memberName]");
            memberNameElement.Change(PlanningPokerData.MemberName);
            var asObserverElement = target.Find("input[name=asObserver]");
            asObserverElement.Change(asObserver);

            var createButtonElement = target.Find("button[type=submit]");
            createButtonElement.Click();

            planningPokerClient.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, asObserver, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public void InitializedWithTeamNameAndMemberName_TeamNameIsPreset()
        {
            var controller = CreateJoinTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<JoinTeamPanel>(
                ComponentParameter.CreateParameter("TeamName", PlanningPokerData.TeamName),
                ComponentParameter.CreateParameter("MemberName", PlanningPokerData.MemberName));

            var teamNameElement = (IHtmlInputElement)target.Find("input[name=teamName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.TeamName, teamNameElement.Value);
            var memberNameElement = (IHtmlInputElement)target.Find("input[name=memberName]").Unwrap();
            Assert.AreEqual(string.Empty, memberNameElement.Value);
        }

        [TestMethod]
        public void InitializedWithTeamNameAndMemberName_ReconnectsToTeam()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var controller = CreateJoinTeamController(
                planningPokerClient: planningPokerClient.Object,
                memberCredentialsStore: memberCredentialsStore.Object);
            InitializeContext(controller);

            memberCredentialsStore.Setup(o => o.GetCredentialsAsync(false))
                .ReturnsAsync(new MemberCredentials { TeamName = PlanningPokerData.TeamName, MemberName = PlanningPokerData.MemberName });
            planningPokerClient.Setup(o => o.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PlanningPokerData.GetReconnectTeamResult());

            using var target = _context.RenderComponent<JoinTeamPanel>(
                ComponentParameter.CreateParameter("TeamName", PlanningPokerData.TeamName),
                ComponentParameter.CreateParameter("MemberName", PlanningPokerData.MemberName));

            memberCredentialsStore.Verify(o => o.GetCredentialsAsync(false), Times.Once());
            planningPokerClient.Verify(o => o.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()));
            memberCredentialsStore.Verify(o => o.GetCredentialsAsync(true), Times.Never());
        }

        [TestMethod]
        public void InitializedWithTeamNameAndMemberNameAndReconnectFails_TeamNameIsPreset()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var controller = CreateJoinTeamController(
                planningPokerClient: planningPokerClient.Object,
                memberCredentialsStore: memberCredentialsStore.Object);
            InitializeContext(controller);

            memberCredentialsStore.Setup(o => o.GetCredentialsAsync(false))
                .ReturnsAsync(new MemberCredentials { TeamName = PlanningPokerData.TeamName, MemberName = PlanningPokerData.MemberName });
            planningPokerClient.Setup(o => o.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            using var target = _context.RenderComponent<JoinTeamPanel>(
                ComponentParameter.CreateParameter("TeamName", PlanningPokerData.TeamName),
                ComponentParameter.CreateParameter("MemberName", PlanningPokerData.MemberName));

            var teamNameElement = (IHtmlInputElement)target.Find("input[name=teamName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.TeamName, teamNameElement.Value);
            var memberNameElement = (IHtmlInputElement)target.Find("input[name=memberName]").Unwrap();
            Assert.AreEqual(string.Empty, memberNameElement.Value);
        }

        [TestMethod]
        public void InitializedWithStoredCredentials_TeamNameAndMemberNameIsPreset()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var controller = CreateJoinTeamController(memberCredentialsStore: memberCredentialsStore.Object);
            InitializeContext(controller);

            memberCredentialsStore.Setup(o => o.GetCredentialsAsync(true))
                .ReturnsAsync(new MemberCredentials { TeamName = PlanningPokerData.TeamName, MemberName = PlanningPokerData.MemberName });

            using var target = _context.RenderComponent<JoinTeamPanel>();

            var teamNameElement = (IHtmlInputElement)target.Find("input[name=teamName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.TeamName, teamNameElement.Value);
            var memberNameElement = (IHtmlInputElement)target.Find("input[name=memberName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.MemberName, memberNameElement.Value);
        }

        private static JoinTeamController CreateJoinTeamController(
            IPlanningPokerClient? planningPokerClient = null,
            IMemberCredentialsStore? memberCredentialsStore = null)
        {
            if (planningPokerClient == null)
            {
                var planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            if (memberCredentialsStore == null)
            {
                var memberCredentialsStoreMock = new Mock<IMemberCredentialsStore>();
                memberCredentialsStore = memberCredentialsStoreMock.Object;
            }

            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var messageBoxService = new Mock<IMessageBoxService>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var navigationManager = new Mock<INavigationManager>();
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            return new JoinTeamController(
                planningPokerClient,
                planningPokerInitializer.Object,
                messageBoxService.Object,
                busyIndicatorService.Object,
                navigationManager.Object,
                memberCredentialsStore,
                serviceTimeProvider.Object);
        }

        private void InitializeContext(JoinTeamController controller, IMessageBoxService? messageBoxService = null)
        {
            if (messageBoxService == null)
            {
                var messageBoxServiceMock = new Mock<IMessageBoxService>();
                messageBoxService = messageBoxServiceMock.Object;
            }

            _context.Services.AddSingleton(controller);
            _context.Services.AddSingleton(messageBoxService);
        }
    }
}
