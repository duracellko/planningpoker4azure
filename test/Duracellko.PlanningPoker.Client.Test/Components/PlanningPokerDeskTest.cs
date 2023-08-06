using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
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
    public sealed class PlanningPokerDeskTest : IDisposable
    {
        private readonly Bunit.TestContext _context = new Bunit.TestContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task InitializedTeamWithScrumMaster_ShowStartEstimationButton()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);
            await controller.InitializeTeam(PlanningPokerData.GetTeamResult(), PlanningPokerData.ScrumMasterName, null);

            using var target = _context.RenderComponent<PlanningPokerDesk>();

            // Team name and user name
            var h2Element = target.Find("div.pokerDeskPanel > div.team-title > h2");
            Assert.AreEqual(2, h2Element.GetElementCount());
            var spanElement = h2Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.TeamName, spanElement.TextContent);

            var h3Element = target.Find("div.pokerDeskPanel > div.team-title > h3");
            Assert.AreEqual(2, h3Element.GetElementCount());
            spanElement = h3Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, spanElement.TextContent);

            // Button to start estimation
            var buttonElement = target.Find("div.actionsBar > p > button");
            CollectionAssert.AreEqual(new[] { "btn", "btn-primary", "col-md-auto", "me-3" }, buttonElement.ClassList.ToList());
            Assert.AreEqual("Start estimation", buttonElement.TextContent.Trim());
        }

        [TestMethod]
        public async Task PlanningPokerStartedWithMember_ShowsAvailableEstimations()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam!.State = TeamState.EstimationInProgress;
            reconnectResult.ScrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.MemberName, null);

            using var target = _context.RenderComponent<PlanningPokerDesk>();

            // Team name and user name
            var h2Element = target.Find("div.pokerDeskPanel > div.team-title > h2");
            Assert.AreEqual(2, h2Element.GetElementCount());
            var spanElement = h2Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.TeamName, spanElement.TextContent);

            var h3Element = target.Find("div.pokerDeskPanel > div.team-title > h3");
            Assert.AreEqual(2, h3Element.GetElementCount());
            spanElement = h3Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.MemberName, spanElement.TextContent);

            // Available estimations
            h3Element = target.Find("div.availableEstimations > h3");
            Assert.AreEqual("Pick estimation", h3Element.TextContent);

            var ulElement = target.Find("div.availableEstimations > ul");
            var liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(13, liElements.Length);
            AssertAvailableEstimation(liElements[0], "0");
            AssertAvailableEstimation(liElements[1], "½");
            AssertAvailableEstimation(liElements[2], "1");
            AssertAvailableEstimation(liElements[3], "2");
            AssertAvailableEstimation(liElements[4], "3");
            AssertAvailableEstimation(liElements[5], "5");
            AssertAvailableEstimation(liElements[6], "8");
            AssertAvailableEstimation(liElements[7], "13");
            AssertAvailableEstimation(liElements[8], "20");
            AssertAvailableEstimation(liElements[9], "40");
            AssertAvailableEstimation(liElements[10], "100");
            AssertAvailableEstimation(liElements[11], "∞");
            AssertAvailableEstimation(liElements[12], "?");

            // Members, who estimated already
            h3Element = target.Find("div.estimationResult > h3");
            Assert.AreEqual("Selected estimates", h3Element.TextContent);
            ulElement = target.Find("div.estimationResult > ul");
            liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(1, liElements.Length);
            AssertSelectedEstimation(liElements[0], PlanningPokerData.ScrumMasterName, string.Empty);
        }

        [TestMethod]
        public async Task PlanningPokerEstimatedWithObserver_ShowsEstimations()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam!.State = TeamState.EstimationFinished;
            reconnectResult.ScrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = 3 }
                }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.ObserverName, null);

            using var target = _context.RenderComponent<PlanningPokerDesk>();

            // Team name and user name
            var h2Element = target.Find("div.pokerDeskPanel > div.team-title > h2");
            Assert.AreEqual(2, h2Element.GetElementCount());
            var spanElement = h2Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.TeamName, spanElement.TextContent);

            var h3Element = target.Find("div.pokerDeskPanel > div.team-title > h3");
            Assert.AreEqual(2, h3Element.GetElementCount());
            spanElement = h3Element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual(PlanningPokerData.ObserverName, spanElement.TextContent);

            // Estimations
            h3Element = target.Find("div.estimationResult > h3");
            Assert.AreEqual("Selected estimates", h3Element.TextContent);
            var ulElement = target.Find("div.estimationResult > ul");
            var liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(2, liElements.Length);
            AssertSelectedEstimation(liElements[0], PlanningPokerData.MemberName, "3");
            AssertSelectedEstimation(liElements[1], PlanningPokerData.ScrumMasterName, "8");
        }

        [TestMethod]
        public async Task PlanningPokerStartedWithTshirtDeck_ShowsAvailableEstimations()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam!.AvailableEstimations = new List<Estimation>
            {
                new Estimation() { Value = -999509 },
                new Estimation() { Value = -999508 },
                new Estimation() { Value = -999507 },
                new Estimation() { Value = -999506 },
                new Estimation() { Value = -999505 }
            };
            reconnectResult.ScrumTeam.State = TeamState.EstimationInProgress;
            reconnectResult.ScrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.ScrumMasterName, Estimated = false },
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.MemberName, null);

            using var target = _context.RenderComponent<PlanningPokerDesk>();

            // Available estimations
            var h3Element = target.Find("div.availableEstimations > h3");
            Assert.AreEqual("Pick estimation", h3Element.TextContent);

            var ulElement = target.Find("div.availableEstimations > ul");
            var liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(5, liElements.Length);
            AssertAvailableEstimation(liElements[0], "XS");
            AssertAvailableEstimation(liElements[1], "S");
            AssertAvailableEstimation(liElements[2], "M");
            AssertAvailableEstimation(liElements[3], "L");
            AssertAvailableEstimation(liElements[4], "XL");

            // Members, who estimated already
            h3Element = target.Find("div.estimationResult > h3");
            Assert.AreEqual("Selected estimates", h3Element.TextContent);
            ulElement = target.Find("div.estimationResult > ul");
            liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(0, liElements.Length);
        }

        [TestMethod]
        public async Task PlanningPokerEstimatedWithRockPaperScissorsLizardSpockDeck_ShowsEstimations()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam!.AvailableEstimations = new List<Estimation>
            {
                new Estimation() { Value = -999909 },
                new Estimation() { Value = -999908 },
                new Estimation() { Value = -999907 },
                new Estimation() { Value = -999906 },
                new Estimation() { Value = -999905 }
            };
            reconnectResult.ScrumTeam.State = TeamState.EstimationFinished;
            reconnectResult.ScrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = -999906 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = -999909 }
                }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.ObserverName, null);

            using var target = _context.RenderComponent<PlanningPokerDesk>();

            // Estimations
            var h3Element = target.Find("div.estimationResult > h3");
            Assert.AreEqual("Selected estimates", h3Element.TextContent);
            var ulElement = target.Find("div.estimationResult > ul");
            var liElements = ulElement.GetElementsByTagName("li");
            Assert.AreEqual(2, liElements.Length);
            AssertSelectedEstimation(liElements[0], PlanningPokerData.MemberName, "💎");
            AssertSelectedEstimation(liElements[1], PlanningPokerData.ScrumMasterName, "🦎");
        }

        private static PlanningPokerController CreatePlanningPokerController()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var timerFactory = new Mock<ITimerFactory>();
            var dateTimeProvider = new DateTimeProviderMock();
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            var timerSettingsRepository = new Mock<ITimerSettingsRepository>();
            var applicationIntegrationService = new Mock<IApplicationIntegrationService>();
            return new PlanningPokerController(
                planningPokerClient.Object,
                busyIndicatorService.Object,
                memberCredentialsStore.Object,
                timerFactory.Object,
                dateTimeProvider,
                serviceTimeProvider.Object,
                timerSettingsRepository.Object,
                applicationIntegrationService.Object);
        }

        private static void AssertAvailableEstimation(IElement element, string estimationText)
        {
            var aElement = element.Children.Single();
            Assert.AreEqual("a", aElement.LocalName);
            Assert.AreEqual(estimationText, aElement.TextContent);
        }

        private static void AssertSelectedEstimation(IElement element, string memberName, string estimationText)
        {
            Assert.AreEqual(2, element.ChildElementCount);

            var spanElement = element.Children[0];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual("estimationItemValue", spanElement.ClassName);
            Assert.AreEqual(estimationText, spanElement.TextContent);

            spanElement = element.Children[1];
            Assert.AreEqual("span", spanElement.LocalName);
            Assert.AreEqual("estimationItemName", spanElement.ClassName);
            Assert.AreEqual(memberName, spanElement.TextContent);
        }

        private void InitializeContext(PlanningPokerController controller, IMessageBoxService? messageBoxService = null)
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
