using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerTest
    {
        private CultureInfo _originalCultureInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalCultureInfo = CultureInfo.CurrentCulture;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_originalCultureInfo != null)
            {
                CultureInfo.CurrentCulture = _originalCultureInfo;
                _originalCultureInfo = null;
            }
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeam_TeamNameIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_MemberName_IsNotScrumMaster()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_MemberNameIsLowerCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, "test member");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_ScrumMasterNameIsUpperCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, "TEST SCRUM MASTER");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_ObserverName_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ObserverName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ObserverName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ObserverType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_LastMessageId_IsMinus1()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(-1, target.LastMessageId);
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeam_ScrumMasterIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster);
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeam_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            var expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            var expectedObservers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.ToList());
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeamWith4MembersAnd3Observers_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Name = "me", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "1st Member", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "XYZ", Type = PlanningPokerData.MemberType });
            scrumTeam.Observers.Add(new TeamMember { Name = "ABC", Type = PlanningPokerData.ObserverType });
            scrumTeam.Observers.Add(new TeamMember { Name = "Hello, World!", Type = PlanningPokerData.ObserverType });

            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            var expectedMembers = new string[] { "1st Member", "me", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            var expectedObservers = new string[] { "ABC", "Hello, World!", PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.ToList());
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeamWithMembersSetToNull_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members = null;
            scrumTeam.Observers = null;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            CollectionAssert.AreEqual(Array.Empty<string>(), target.Members.ToList());
            CollectionAssert.AreEqual(Array.Empty<string>(), target.Observers.ToList());
            Assert.IsNotNull(target.ScrumTeam.Members);
            Assert.AreEqual(0, target.ScrumTeam.Members.Count);
            Assert.IsNotNull(target.ScrumTeam.Observers);
            Assert.AreEqual(0, target.ScrumTeam.Observers.Count);
        }

        [TestMethod]
        public void InitializeTeam_ScrumTeam_AvailableEstimationsAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            var expectedEstimations = new double?[] { 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100, double.PositiveInfinity, null };
            CollectionAssert.AreEqual(expectedEstimations, target.AvailableEstimations.ToList());
        }

        [TestMethod]
        public void InitializeTeam_ScrumMasterAndInitialState_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_ScrumMasterAndEstimationInProgress_CanCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_ScrumMasterAndEstimationFinished_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_ScrumMasterAndEstimationCanceled_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_MemberAndInitialState_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_MemberAndEstimationInProgress_CannotCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_MemberAndEstimationFinished_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public void InitializeTeam_MemberAndEstimationCanceled_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var target = CreateController();

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_StartEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartEstimation_CannotStartEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.StartEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_CancelEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CancelEstimation_CannotCancelEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.CancelEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task SelectEstimation_5AndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, 5, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_PositiveInfinityAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(double.PositiveInfinity);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, double.PositiveInfinity, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_NullAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(null);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, null, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_CannotSelectEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task SelectEstimation_CanSelectEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<double?>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            var result = target.SelectEstimation(5);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        private static PlanningPokerController CreateController(IPlanningPokerClient planningPokerClient = null, IBusyIndicatorService busyIndicator = null)
        {
            if (planningPokerClient == null)
            {
                var planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            if (busyIndicator == null)
            {
                var busyIndicatorMock = new Mock<IBusyIndicatorService>();
                busyIndicator = busyIndicatorMock.Object;
            }

            return new PlanningPokerController(planningPokerClient, busyIndicator);
        }
    }
}
