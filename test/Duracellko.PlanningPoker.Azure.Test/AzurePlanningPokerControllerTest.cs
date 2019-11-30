using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Mock objects do not need to be disposed.")]
    public class AzurePlanningPokerControllerTest
    {
        [TestMethod]
        public void ObservableMessages_TeamCreated_ScrumTeamCreatedMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            target.CreateScrumTeam("test", "master");
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.TeamCreated, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_MemberJoined_ScrumTeamMemberMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.Join("member", false);
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            var memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("member", memberMessage.MemberName);
            Assert.AreEqual<string>("Member", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_MemberDisconnected_ScrumTeamMemberMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.Disconnect("master");
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            var memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<string>("ScrumMaster", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_MemberUpdateActivity_ScrumTeamMemberMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.UpdateActivity();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberActivity, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            var memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<string>("ScrumMaster", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_EstimationStarted_ScrumTeamMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.StartEstimation();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_EstimationCanceled_ScrumTeamMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");
            teamLock.Team.ScrumMaster.StartEstimation();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.CancelEstimation();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_MemberEstimated_ScrumTeamMemberMessage()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            var messages = new List<ScrumTeamMessage>();
            var teamLock = target.CreateScrumTeam("test", "master");
            teamLock.Team.ScrumMaster.StartEstimation();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.Estimation = new Estimation(3.0);
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(2, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberEstimationMessage));
            var memberMessage = (ScrumTeamMemberEstimationMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<double?>(3.0, memberMessage.Estimation);
        }

        [TestMethod]
        public void CreateScrumteam_AfterInitialization_CreatesNewTeam()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();

            // Act
            var result = target.CreateScrumTeam("test", "master");

            // Verify
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Team);
            Assert.AreEqual<string>("test", result.Team.Name);
            Assert.AreEqual<string>("master", result.Team.ScrumMaster.Name);
        }

        [TestMethod]
        public void CreateScrumteam_InitializationTeamListIsNotSet_WaitForInitializationTeamList()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();

            // Act
            var task = Task.Factory.StartNew<IScrumTeamLock>(() => target.CreateScrumTeam("test", "master"), default(CancellationToken), TaskCreationOptions.None, TaskScheduler.Default);
            Assert.IsFalse(task.IsCompleted);
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            target.SetTeamsInitializingList(Enumerable.Empty<string>());
            Assert.IsTrue(task.Wait(1000));

            // Verify
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumteam_TeamNameIsInInitializationTeamList_ArgumentException()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test" });

            // Act
            target.CreateScrumTeam("test", "master");
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void CreateScrumteam_InitializationTimeout_Exception()
        {
            // Arrange
            var configuration = new AzurePlanningPokerConfiguration() { InitializationTimeout = 1 };
            var target = CreateAzurePlanningPokerController(configuration: configuration);

            // Act
            target.CreateScrumTeam("test", "master");
        }

        [TestMethod]
        public void GetScrumTeam_AfterInitialization_GetsExistingTeam()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("test team", "master"))
            {
                team = teamLock.Team;
            }

            // Act
            var result = target.GetScrumTeam("test team");

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        public void GetScrumTeam_TeamIsNotInitialized_WaitForTeamInitialization()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test team", "team2" });

            // Act
            var task = Task.Factory.StartNew<IScrumTeamLock>(() => target.GetScrumTeam("test team"), default(CancellationToken), TaskCreationOptions.None, TaskScheduler.Default);
            Assert.IsFalse(task.IsCompleted);
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            target.InitializeScrumTeam(new ScrumTeam("test team"));
            Assert.IsTrue(task.Wait(1000));

            // Verify
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void GetScrumTeam_TeamIsNotWaitingForInitialization_ReturnsTeam()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test team", "team2" });
            target.InitializeScrumTeam(new ScrumTeam("test team"));

            // Act
            var result = target.GetScrumTeam("test team");

            // Verify
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_InitializationTimeout_ArgumentException()
        {
            // Arrange
            var configuration = new AzurePlanningPokerConfiguration() { InitializationTimeout = 1 };
            var target = CreateAzurePlanningPokerController(configuration: configuration);

            // Act
            target.GetScrumTeam("test team");
        }

        [TestMethod]
        public void SetTeamsInitializingList_TeamSpeacified_DeleteAllFromRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.DeleteAll());
            var target = CreateAzurePlanningPokerController(repository: repository.Object);

            // Act
            target.SetTeamsInitializingList(new string[] { "team" });

            // Verify
            repository.Verify(r => r.DeleteAll());
        }

        [TestMethod]
        public void SetTeamsInitializingList_AfterEndInitialization_NotDeleteAnythingFromRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            var target = CreateAzurePlanningPokerController(repository: repository.Object);
            target.EndInitialization();

            // Act
            target.SetTeamsInitializingList(new string[] { "team" });

            // Verify
            repository.Verify(r => r.DeleteAll(), Times.Never());
        }

        [TestMethod]
        public void InitializeScrumTeam_TeamSpeacified_TeamAddedToController()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            var team = new ScrumTeam("team");
            target.SetTeamsInitializingList(new string[] { "team" });

            // Act
            target.InitializeScrumTeam(team);

            // Verify
            var result = target.GetScrumTeam("team");
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        public void InitializeScrumTeam_TeamSpecified_TeamCreatedMessageIsNotSent()
        {
            // Arrange
            var target = CreateAzurePlanningPokerController();
            var team = new ScrumTeam("team");
            target.SetTeamsInitializingList(new string[] { "team" });
            ScrumTeamMessage message = null;
            target.ObservableMessages.Subscribe(m => message = m);

            // Act
            target.InitializeScrumTeam(team);

            // Verify
            Assert.IsNull(message);
        }

        private static AzurePlanningPokerController CreateAzurePlanningPokerController(
            DateTimeProvider dateTimeProvider = null,
            IAzurePlanningPokerConfiguration configuration = null,
            IScrumTeamRepository repository = null,
            TaskProvider taskProvider = null,
            ILogger<Controllers.PlanningPokerController> logger = null)
        {
            if (logger == null)
            {
                logger = Mock.Of<ILogger<Controllers.PlanningPokerController>>();
            }

            return new AzurePlanningPokerController(dateTimeProvider, configuration, repository, taskProvider, logger);
        }
    }
}
