using System;
using System.Threading;
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
        private Bunit.TestContext _context = new Bunit.TestContext();

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
        public void ClickCreate_SendsCreateTeamRequest()
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

        private static CreateTeamController CreateCreateTeamController(IPlanningPokerClient? planningPokerClient = null)
        {
            if (planningPokerClient == null)
            {
                var planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            var messageBoxService = new Mock<IMessageBoxService>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var navigationManager = new Mock<INavigationManager>();
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            return new CreateTeamController(
                planningPokerClient,
                planningPokerInitializer.Object,
                messageBoxService.Object,
                busyIndicatorService.Object,
                navigationManager.Object,
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
