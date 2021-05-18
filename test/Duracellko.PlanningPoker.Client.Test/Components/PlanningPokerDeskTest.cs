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
            Assert.AreEqual(21, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 21);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[2], "div", 11);
            AssertFrame.Attribute(frames[3], "class", "team-title");
            AssertFrame.Element(frames[4], "h2", 4);
            AssertFrame.Markup(frames[5], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[6], "span", 2);
            AssertFrame.Text(frames[7], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[9], "h3", 4);
            AssertFrame.Markup(frames[10], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[11], "span", 2);
            AssertFrame.Text(frames[12], PlanningPokerData.ScrumMasterName);

            // Button to start estimation
            AssertFrame.Element(frames[13], "div", 8);
            AssertFrame.Attribute(frames[14], "class", "actionsBar");
            AssertFrame.Element(frames[15], "p", 6);
            AssertFrame.Element(frames[16], "button", 5);
            AssertFrame.Attribute(frames[17], "type", "button");
            AssertFrame.Attribute(frames[18], "onclick");
            AssertFrame.Attribute(frames[19], "class", "btn btn-primary mr-3");
            AssertFrame.Markup(frames[20], $"<span class=\"oi oi-media-play mr-1\" aria-hidden=\"true\"></span> Start estimation{_newLine}                    ");
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
            Assert.AreEqual(81, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 81);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[2], "div", 11);
            AssertFrame.Attribute(frames[3], "class", "team-title");
            AssertFrame.Element(frames[4], "h2", 4);
            AssertFrame.Markup(frames[5], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[6], "span", 2);
            AssertFrame.Text(frames[7], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[9], "h3", 4);
            AssertFrame.Markup(frames[10], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[11], "span", 2);
            AssertFrame.Text(frames[12], PlanningPokerData.MemberName);

            // Available estimations
            AssertFrame.Element(frames[13], "div", 56);
            AssertFrame.Attribute(frames[14], "class", "availableEstimations");
            AssertFrame.Markup(frames[15], $"<h3>Pick estimation</h3>{_newLine}            ");
            AssertFrame.Element(frames[16], "ul", 53);
            AssertAvailableEstimation(frames, 17, "0");
            AssertAvailableEstimation(frames, 21, "½");
            AssertAvailableEstimation(frames, 25, "1");
            AssertAvailableEstimation(frames, 29, "2");
            AssertAvailableEstimation(frames, 33, "3");
            AssertAvailableEstimation(frames, 37, "5");
            AssertAvailableEstimation(frames, 41, "8");
            AssertAvailableEstimation(frames, 45, "13");
            AssertAvailableEstimation(frames, 49, "20");
            AssertAvailableEstimation(frames, 53, "40");
            AssertAvailableEstimation(frames, 57, "100");
            AssertAvailableEstimation(frames, 61, "∞");
            AssertAvailableEstimation(frames, 65, "?");

            // Members, who estimated already
            AssertFrame.Element(frames[69], "div", 12);
            AssertFrame.Attribute(frames[70], "class", "estimationResult");
            AssertFrame.Markup(frames[71], $"<h3>Selected estimates</h3>");
            AssertFrame.Element(frames[72], "ul", 9);
            AssertSelectedEstimation(frames, 73, PlanningPokerData.ScrumMasterName, string.Empty);
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
            Assert.AreEqual(41, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 41);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[2], "div", 11);
            AssertFrame.Attribute(frames[3], "class", "team-title");
            AssertFrame.Element(frames[4], "h2", 4);
            AssertFrame.Markup(frames[5], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-people\" title=\"Team\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[6], "span", 2);
            AssertFrame.Text(frames[7], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[9], "h3", 4);
            AssertFrame.Markup(frames[10], $"<span class=\"badge badge-secondary\"><span class=\"oi oi-person\" title=\"User\" aria-hidden=\"true\"></span></span>{_newLine}            ");
            AssertFrame.Element(frames[11], "span", 2);
            AssertFrame.Text(frames[12], PlanningPokerData.ObserverName);

            // Button to start estimation
            AssertFrame.Element(frames[13], "div", 8);
            AssertFrame.Attribute(frames[14], "class", "actionsBar");
            AssertFrame.Element(frames[15], "p", 6);
            AssertFrame.Element(frames[16], "button", 5);
            AssertFrame.Attribute(frames[17], "type", "button");
            AssertFrame.Attribute(frames[18], "onclick");
            AssertFrame.Attribute(frames[19], "class", "btn btn-secondary mr-3");
            AssertFrame.Markup(frames[20], $"<span class=\"oi oi-calculator mr-1\" aria-hidden=\"true\"></span> Show average{_newLine}                    ");

            // Estimations
            AssertFrame.Element(frames[21], "div", 20);
            AssertFrame.Attribute(frames[22], "class", "estimationResult");
            AssertFrame.Markup(frames[23], $"<h3>Selected estimates</h3>");
            AssertFrame.Element(frames[24], "ul", 17);
            AssertSelectedEstimation(frames, 25, PlanningPokerData.MemberName, "3");
            AssertSelectedEstimation(frames, 33, PlanningPokerData.ScrumMasterName, "8");
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
