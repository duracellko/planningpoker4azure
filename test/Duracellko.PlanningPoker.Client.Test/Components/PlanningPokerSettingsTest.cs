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
    public sealed class PlanningPokerSettingsTest : IDisposable
    {
        private Bunit.TestContext _context = new Bunit.TestContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task Initialized_TimerIsSetTo5Minutes()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);
            await controller.InitializeTeam(PlanningPokerData.GetTeamResult(), PlanningPokerData.ScrumMasterName);

            using var target = _context.RenderComponent<PlanningPokerSettings>();

            // Timer input
            var inputGroupElement = target.Find("div#planningPokerSettingsModal > div.modal-dialog > div.modal-content > div.modal-body > form > fieldset > div.mb-3 > div.input-group");
            var selectElements = inputGroupElement.GetElementsByTagName("select");
            Assert.AreEqual(2, selectElements.Length);

            var minutesElement = (IHtmlSelectElement)selectElements[0];
            Assert.AreEqual("minutes", minutesElement.GetAttribute("aria-label"));
            Assert.AreEqual("5", minutesElement.Value);

            var secondsElement = (IHtmlSelectElement)selectElements[1];
            Assert.AreEqual("seconds", secondsElement.GetAttribute("aria-label"));
            Assert.AreEqual("0", secondsElement.Value);

            Assert.AreEqual(TimeSpan.FromMinutes(5), controller.TimerDuration);
        }

        [TestMethod]
        public async Task SetMinutesTo0_TimerIsSetTo1Second()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);
            await controller.InitializeTeam(PlanningPokerData.GetTeamResult(), PlanningPokerData.ScrumMasterName);

            using var target = _context.RenderComponent<PlanningPokerSettings>();

            // Timer input
            var inputGroupElement = target.Find("div#planningPokerSettingsModal > div.modal-dialog > div.modal-content > div.modal-body > form > fieldset > div.mb-3 > div.input-group");
            var selectElements = inputGroupElement.GetElementsByTagName("select");
            var minutesElement = (IHtmlSelectElement)selectElements[0];
            minutesElement.Change("0");

            selectElements = inputGroupElement.GetElementsByTagName("select");
            minutesElement = (IHtmlSelectElement)selectElements[0];
            var secondsElement = (IHtmlSelectElement)selectElements[1];

            Assert.AreEqual("0", minutesElement.Value);
            Assert.AreEqual("1", secondsElement.Value);
            Assert.AreEqual(TimeSpan.FromSeconds(1), controller.TimerDuration);
        }

        [TestMethod]
        public async Task SetSecondsTo0_TimerIsSetTo1Minute()
        {
            using var controller = CreatePlanningPokerController();
            InitializeContext(controller);
            await controller.InitializeTeam(PlanningPokerData.GetTeamResult(), PlanningPokerData.ScrumMasterName);

            using var target = _context.RenderComponent<PlanningPokerSettings>();

            // Timer input
            var inputGroupElement = target.Find("div#planningPokerSettingsModal > div.modal-dialog > div.modal-content > div.modal-body > form > fieldset > div.mb-3 > div.input-group");
            var selectElements = inputGroupElement.GetElementsByTagName("select");
            var minutesElement = (IHtmlSelectElement)selectElements[0];
            minutesElement.Change("0");

            selectElements = inputGroupElement.GetElementsByTagName("select");
            var secondsElement = (IHtmlSelectElement)selectElements[1];
            secondsElement.Change("0");

            selectElements = inputGroupElement.GetElementsByTagName("select");
            minutesElement = (IHtmlSelectElement)selectElements[0];
            secondsElement = (IHtmlSelectElement)selectElements[1];

            Assert.AreEqual("1", minutesElement.Value);
            Assert.AreEqual("0", secondsElement.Value);
            Assert.AreEqual(TimeSpan.FromMinutes(1), controller.TimerDuration);
        }

        private static PlanningPokerController CreatePlanningPokerController()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var timerFactory = new Mock<ITimerFactory>();
            var dateTimeProvider = new DateTimeProviderMock();
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            return new PlanningPokerController(
                planningPokerClient.Object,
                busyIndicatorService.Object,
                memberCredentialsStore.Object,
                timerFactory.Object,
                dateTimeProvider,
                serviceTimeProvider.Object);
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
