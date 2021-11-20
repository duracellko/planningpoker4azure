using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Components;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [TestClass]
    public class PlanningPokerDeskTest
    {
        private static readonly string _newLine = Environment.NewLine;

        [TestMethod]
        public async Task InitializedTeamWithScrumMaster_ShowStartEstimationButton()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            await controller.InitializeTeam(PlanningPokerData.GetTeamResult(), PlanningPokerData.ScrumMasterName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            var componentId = renderer.AssignRootComponentId(target);
            await renderer.RenderRootComponentAsync(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            var frames = renderer.Batches[0].ReferenceFrames;
            Assert.IsNotNull(frames);
            Assert.AreEqual(426, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 426);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[400], "div", 11);
            AssertFrame.Attribute(frames[401], "class", "team-title");
            AssertFrame.Element(frames[402], "h2", 4);
            AssertFrame.Markup(frames[403], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[404], "span", 2);
            AssertFrame.Text(frames[405], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[407], "h3", 4);
            AssertFrame.Markup(frames[408], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[409], "span", 2);
            AssertFrame.Text(frames[410], PlanningPokerData.ScrumMasterName);

            // Button to start estimation
            AssertFrame.Element(frames[411], "div", 15);
            AssertFrame.Attribute(frames[412], "class", "actionsBar");
            AssertFrame.Element(frames[413], "p", 13);
            AssertFrame.Element(frames[414], "button", 5);
            AssertFrame.Attribute(frames[415], "type", "button");
            AssertFrame.Attribute(frames[416], "onclick");
            AssertFrame.Attribute(frames[417], "class", "btn btn-primary mr-3");
            AssertFrame.Markup(frames[418], $"<span class=\"oi oi-media-play mr-1\" aria-hidden=\"true\"></span> Start estimation{_newLine}                    ");
        }

        [TestMethod]
        public async Task PlanningPokerStartedWithMember_ShowsAvailableEstimations()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam!.State = TeamState.EstimationInProgress;
            reconnectResult.ScrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimationParticipantStatus() { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.MemberName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            var componentId = renderer.AssignRootComponentId(target);
            await renderer.RenderRootComponentAsync(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            var frames = renderer.Batches[0].ReferenceFrames;
            Assert.IsNotNull(frames);
            Assert.AreEqual(489, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 489);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[400], "div", 11);
            AssertFrame.Attribute(frames[401], "class", "team-title");
            AssertFrame.Element(frames[402], "h2", 4);
            AssertFrame.Markup(frames[403], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[404], "span", 2);
            AssertFrame.Text(frames[405], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[407], "h3", 4);
            AssertFrame.Markup(frames[408], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[409], "span", 2);
            AssertFrame.Text(frames[410], PlanningPokerData.MemberName);

            // Available estimations
            AssertFrame.Element(frames[411], "div", 56);
            AssertFrame.Attribute(frames[412], "class", "availableEstimations");
            AssertFrame.Markup(frames[413], $"<h3>Pick estimation</h3>{_newLine}            ");
            AssertFrame.Element(frames[414], "ul", 53);
            AssertAvailableEstimation(frames, 415, "0");
            AssertAvailableEstimation(frames, 419, "½");
            AssertAvailableEstimation(frames, 423, "1");
            AssertAvailableEstimation(frames, 427, "2");
            AssertAvailableEstimation(frames, 431, "3");
            AssertAvailableEstimation(frames, 435, "5");
            AssertAvailableEstimation(frames, 439, "8");
            AssertAvailableEstimation(frames, 443, "13");
            AssertAvailableEstimation(frames, 447, "20");
            AssertAvailableEstimation(frames, 451, "40");
            AssertAvailableEstimation(frames, 455, "100");
            AssertAvailableEstimation(frames, 459, "∞");
            AssertAvailableEstimation(frames, 463, "?");

            // Members, who estimated already
            AssertFrame.Element(frames[477], "div", 12);
            AssertFrame.Attribute(frames[478], "class", "estimationResult");
            AssertFrame.Markup(frames[479], $"<h3>Selected estimates</h3>");
            AssertFrame.Element(frames[480], "ul", 9);
            AssertSelectedEstimation(frames, 481, PlanningPokerData.ScrumMasterName, string.Empty);
        }

        [TestMethod]
        public async Task PlanningPokerEstimatedWithObserver_ShowsEstimations()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

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
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.ObserverName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            var componentId = renderer.AssignRootComponentId(target);
            await renderer.RenderRootComponentAsync(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            var frames = renderer.Batches[0].ReferenceFrames;
            Assert.IsNotNull(frames);
            Assert.AreEqual(439, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 439);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[400], "div", 11);
            AssertFrame.Attribute(frames[401], "class", "team-title");
            AssertFrame.Element(frames[402], "h2", 4);
            AssertFrame.Markup(frames[403], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[404], "span", 2);
            AssertFrame.Text(frames[405], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[407], "h3", 4);
            AssertFrame.Markup(frames[408], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[409], "span", 2);
            AssertFrame.Text(frames[410], PlanningPokerData.ObserverName);

            // Button to start estimation
            AssertFrame.Element(frames[411], "div", 8);
            AssertFrame.Attribute(frames[412], "class", "actionsBar");
            AssertFrame.Element(frames[413], "p", 6);
            AssertFrame.Element(frames[414], "button", 5);
            AssertFrame.Attribute(frames[415], "type", "button");
            AssertFrame.Attribute(frames[416], "onclick");
            AssertFrame.Attribute(frames[417], "class", "btn btn-secondary mr-3");
            AssertFrame.Markup(frames[418], $"<span class=\"oi oi-calculator mr-1\" aria-hidden=\"true\"></span> Show average{_newLine}                    ");

            // Estimations
            AssertFrame.Element(frames[419], "div", 20);
            AssertFrame.Attribute(frames[420], "class", "estimationResult");
            AssertFrame.Markup(frames[421], $"<h3>Selected estimates</h3>");
            AssertFrame.Element(frames[422], "ul", 17);
            AssertSelectedEstimation(frames, 423, PlanningPokerData.MemberName, "3");
            AssertSelectedEstimation(frames, 431, PlanningPokerData.ScrumMasterName, "8");
        }

        private static IServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<TestRenderer>();
            serviceCollection.AddSingleton<PlanningPokerController>();
            serviceCollection.AddSingleton(new Mock<IMessageBoxService>().Object);
            serviceCollection.AddSingleton(new Mock<IPlanningPokerClient>().Object);
            serviceCollection.AddSingleton(new Mock<IBusyIndicatorService>().Object);
            serviceCollection.AddSingleton(new Mock<IMemberCredentialsStore>().Object);
            serviceCollection.AddSingleton(new Mock<ITimerFactory>().Object);
            serviceCollection.AddSingleton<DateTimeProvider>(new DateTimeProviderMock());
            serviceCollection.AddSingleton(new Mock<IServiceTimeProvider>().Object);
            return serviceCollection.BuildServiceProvider();
        }

        private static void AssertAvailableEstimation(IList<RenderTreeFrame> frames, int index, string estimationText)
        {
            AssertFrame.Element(frames[index], "li", 4);
            AssertFrame.Element(frames[index + 1], "a", 3);
            AssertFrame.Attribute(frames[index + 2], "onclick");
            AssertFrame.Text(frames[index + 3], estimationText);
        }

        private static void AssertSelectedEstimation(IList<RenderTreeFrame> frames, int index, string memberName, string estimationText)
        {
            AssertFrame.Element(frames[index], "li", 8);
            AssertFrame.Element(frames[index + 1], "span", 3);
            AssertFrame.Attribute(frames[index + 2], "class", "estimationItemValue");
            AssertFrame.Text(frames[index + 3], estimationText);
            AssertFrame.Element(frames[index + 5], "span", 3);
            AssertFrame.Attribute(frames[index + 6], "class", "estimationItemName");
            AssertFrame.Text(frames[index + 7], memberName);
        }
    }
}
