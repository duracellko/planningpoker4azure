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
        [TestMethod]
        public async Task InitializedTeamWithScrumMaster_ShowStartEstimationButton()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            await controller.InitializeTeam(PlanningPokerData.GetScrumTeam(), PlanningPokerData.ScrumMasterName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            var componentId = renderer.AssignRootComponentId(target);
            await renderer.RenderRootComponentAsync(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            var frames = renderer.Batches[0].ReferenceFrames;
            Assert.AreEqual(39, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 39);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\r\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\r\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.ScrumMasterName);

            // Button to start estimation
            AssertFrame.Element(frames[23], "div", 14);
            AssertFrame.Attribute(frames[24], "class", "actionsBar");
            AssertFrame.Element(frames[26], "p", 10);
            AssertFrame.Element(frames[29], "a", 4);
            AssertFrame.Attribute(frames[30], "onclick");
            AssertFrame.Attribute(frames[31], "class", "btn btn-default");
            AssertFrame.Markup(frames[32], "\r\n                        <span class=\"glyphicon glyphicon-play\"></span> Start estimation\r\n                    ");
        }

        [TestMethod]
        public async Task PlanningPokerStartedWithMember_ShowsAvailableEstimations()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam.State = TeamState.EstimationInProgress;
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
            Assert.AreEqual(133, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 133);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\r\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\r\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.MemberName);

            // Available estimations
            AssertFrame.Element(frames[22], "div", 86);
            AssertFrame.Attribute(frames[23], "class", "availableEstimations");
            AssertFrame.Markup(frames[25], "<h3>Pick estimation</h3>\r\n            ");
            AssertFrame.Element(frames[26], "ul", 81);
            AssertAvailableEstimation(frames, 29, "0");
            AssertAvailableEstimation(frames, 35, "½");
            AssertAvailableEstimation(frames, 41, "1");
            AssertAvailableEstimation(frames, 47, "2");
            AssertAvailableEstimation(frames, 53, "3");
            AssertAvailableEstimation(frames, 59, "5");
            AssertAvailableEstimation(frames, 65, "8");
            AssertAvailableEstimation(frames, 71, "13");
            AssertAvailableEstimation(frames, 77, "20");
            AssertAvailableEstimation(frames, 83, "40");
            AssertAvailableEstimation(frames, 89, "100");
            AssertAvailableEstimation(frames, 95, "∞");
            AssertAvailableEstimation(frames, 101, "?");

            // Members, who estimated already
            AssertFrame.Element(frames[112], "div", 20);
            AssertFrame.Attribute(frames[113], "class", "estimationResult");
            AssertFrame.Markup(frames[115], "<h3>Selected estimates</h3>\r\n            ");
            AssertFrame.Element(frames[116], "ul", 15);
            AssertSelectedEstimation(frames, 119, PlanningPokerData.ScrumMasterName, string.Empty);
        }

        [TestMethod]
        public async Task PlanningPokerEstimatedWithObserver_ShowsEstimations()
        {
            var serviceProvider = CreateServiceProvider();
            var renderer = serviceProvider.GetRequiredService<TestRenderer>();
            var controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            var reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam.State = TeamState.EstimationFinished;
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
            Assert.AreEqual(57, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 57);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\r\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\r\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.ObserverName);

            // Estimations
            AssertFrame.Element(frames[24], "div", 32);
            AssertFrame.Attribute(frames[25], "class", "estimationResult");
            AssertFrame.Markup(frames[27], "<h3>Selected estimates</h3>\r\n            ");
            AssertFrame.Element(frames[28], "ul", 27);
            AssertSelectedEstimation(frames, 31, PlanningPokerData.MemberName, "3");
            AssertSelectedEstimation(frames, 43, PlanningPokerData.ScrumMasterName, "8");
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
            AssertFrame.Element(frames[index], "li", 10);
            AssertFrame.Element(frames[index + 2], "span", 3);
            AssertFrame.Attribute(frames[index + 3], "class", "estimationItemValue");
            AssertFrame.Text(frames[index + 4], estimationText);
            AssertFrame.Element(frames[index + 6], "span", 3);
            AssertFrame.Attribute(frames[index + 7], "class", "estimationItemName");
            AssertFrame.Text(frames[index + 8], memberName);
        }
    }
}
