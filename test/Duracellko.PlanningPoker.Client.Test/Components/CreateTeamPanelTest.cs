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
using Duracellko.PlanningPoker.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [TestClass]
    public sealed class CreateTeamPanelTest : IDisposable
    {
        private readonly Bunit.TestContext _context = new Bunit.TestContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void Initialized_NoValidationErrors()
        {
            var controller = CreateCreateTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            var scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            Assert.AreEqual("form-control", scrumMasterNameElement.ClassName);
            validationElements = scrumMasterNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetTeamNameAndScrumMasterName_NoValidationErrors()
        {
            var controller = CreateCreateTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            scrumMasterNameElement.Change(PlanningPokerData.ScrumMasterName);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            Assert.AreEqual("form-control", scrumMasterNameElement.ClassName);
            validationElements = scrumMasterNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetTeamNameToEmpty_RequiredValidationError()
        {
            var controller = CreateCreateTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            scrumMasterNameElement.Change(PlanningPokerData.ScrumMasterName);

            teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(string.Empty);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control is-invalid", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(1, validationElements.Length);
            Assert.AreEqual("Required", validationElements[0].TextContent);

            scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            Assert.AreEqual("form-control", scrumMasterNameElement.ClassName);
            validationElements = scrumMasterNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);
        }

        [TestMethod]
        public void SetScrumMasterNameToEmpty_RequiredValidationError()
        {
            var controller = CreateCreateTeamController();
            InitializeContext(controller);

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            scrumMasterNameElement.Change(PlanningPokerData.ScrumMasterName);

            scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            scrumMasterNameElement.Change(string.Empty);

            teamNameElement = target.Find("input[name=teamName]");
            Assert.AreEqual("form-control", teamNameElement.ClassName);
            var validationElements = teamNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(0, validationElements.Length);

            scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            Assert.AreEqual("form-control is-invalid", scrumMasterNameElement.ClassName);
            validationElements = scrumMasterNameElement.ParentElement!.GetElementsByClassName("invalid-feedback");
            Assert.AreEqual(1, validationElements.Length);
            Assert.AreEqual("Required", validationElements[0].TextContent);
        }

        [TestMethod]
        public void ClickCreateButton_SendsCreateTeamRequest()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var controller = CreateCreateTeamController(planningPokerClient: planningPokerClient.Object);
            InitializeContext(controller);

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = target.Find("input[name=teamName]");
            teamNameElement.Change(PlanningPokerData.TeamName);
            var scrumMasterNameElement = target.Find("input[name=scrumMasterName]");
            scrumMasterNameElement.Change(PlanningPokerData.ScrumMasterName);

            var createButtonElement = target.Find("button[type=submit]");
            createButtonElement.Click();

            planningPokerClient.Verify(o => o.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public void InitializedWithStoredCredentials_TeamNameAndMemberNameIsPreset()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var controller = CreateCreateTeamController(memberCredentialsStore: memberCredentialsStore.Object);
            InitializeContext(controller);

            memberCredentialsStore.Setup(o => o.GetCredentialsAsync(true))
                .ReturnsAsync(new MemberCredentials { TeamName = PlanningPokerData.TeamName, MemberName = PlanningPokerData.MemberName });

            using var target = _context.RenderComponent<CreateTeamPanel>();

            var teamNameElement = (IHtmlInputElement)target.Find("input[name=teamName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.TeamName, teamNameElement.Value);
            var memberNameElement = (IHtmlInputElement)target.Find("input[name=scrumMasterName]").Unwrap();
            Assert.AreEqual(PlanningPokerData.MemberName, memberNameElement.Value);
        }

        [TestMethod]
        public void InitializedWithAutoConnect_TeamNameAndMemberNameIsNotChanged()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var url = "http://planningpoker.duracellko.net/Index?AutoConnect=True&CallbackUri=http%3A%2F%2Fmy.app%2F&CallbackReference=2";
            var controller = CreateCreateTeamController(memberCredentialsStore: memberCredentialsStore.Object, url: url);
            InitializeContext(controller);

            memberCredentialsStore.Setup(o => o.GetCredentialsAsync(true))
                .ReturnsAsync(new MemberCredentials { TeamName = PlanningPokerData.TeamName, MemberName = PlanningPokerData.MemberName });

            using var target = _context.RenderComponent<CreateTeamPanel>(
                ComponentParameter.CreateParameter(nameof(CreateTeamPanel.TeamName), "Hello"),
                ComponentParameter.CreateParameter(nameof(CreateTeamPanel.ScrumMasterName), "World"));

            var teamNameElement = (IHtmlInputElement)target.Find("input[name=teamName]").Unwrap();
            Assert.AreEqual("Hello", teamNameElement.Value);
            var memberNameElement = (IHtmlInputElement)target.Find("input[name=scrumMasterName]").Unwrap();
            Assert.AreEqual("World", memberNameElement.Value);
        }

        private static CreateTeamController CreateCreateTeamController(
            IPlanningPokerClient? planningPokerClient = null,
            IMemberCredentialsStore? memberCredentialsStore = null,
            string? url = null)
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

            navigationManager.SetupGet(o => o.Uri).Returns(url ?? "http://planningpoker.duracellko.net/");

            return new CreateTeamController(
                planningPokerClient,
                planningPokerInitializer.Object,
                messageBoxService.Object,
                busyIndicatorService.Object,
                navigationManager.Object,
                memberCredentialsStore,
                serviceTimeProvider.Object);
        }

        private void InitializeContext(CreateTeamController controller, IMessageBoxService? messageBoxService = null)
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
