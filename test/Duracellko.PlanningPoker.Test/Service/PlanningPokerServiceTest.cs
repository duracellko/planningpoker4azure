using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Test.Service
{
    [TestClass]
    public class PlanningPokerServiceTest
    {
        private const string TeamName = "test team";
        private const string ScrumMasterName = "master";
        private const string MemberName = "member";
        private const string ObserverName = "observer";

        private const string LongTeamName = "ttttttttttttttttttttttttttttttttttttttttttttttttttt";
        private const string LongMemberName = "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm";

        [TestMethod]
        public void Constructor_PlanningPoker_PlanningPokerPropertyIsSet()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);

            // Act
            var result = new PlanningPokerService(planningPoker.Object);

            // Verify
            Assert.AreEqual<D.IPlanningPoker>(planningPoker.Object, result.PlanningPoker);
        }

        [TestMethod]
        public void Constructor_Null_ArgumentNullException()
        {
            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new PlanningPokerService(null));
        }

        [TestMethod]
        public void CreateTeam_TeamNameAndScrumMasterName_ReturnsCreatedTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName)).Returns(teamLock.Object).Verifiable();

            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.CreateTeam(TeamName, ScrumMasterName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual<string>(typeof(D.ScrumMaster).Name, result.ScrumMaster.Type);
        }

        [TestMethod]
        public void CreateTeam_TeamNameAndScrumMasterName_ReturnsTeamWithAvilableEstimations()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName)).Returns(teamLock.Object).Verifiable();

            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.CreateTeam(TeamName, ScrumMasterName).Value;

            // Verify
            Assert.IsNotNull(result.AvailableEstimations);
            var expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, Estimation.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.AvailableEstimations.Select(e => e.Value).ToList());
        }

        [TestMethod]
        public void CreateTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.CreateTeam(null, ScrumMasterName));
        }

        [TestMethod]
        public void CreateTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.CreateTeam(TeamName, null));
        }

        [TestMethod]
        public void CreateTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.CreateTeam(LongTeamName, ScrumMasterName));
        }

        [TestMethod]
        public void CreateTeam_ScrumMasterNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.CreateTeam(TeamName, LongMemberName));
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMember_ReturnsTeamJoined()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.JoinTeam(TeamName, MemberName, false).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.IsNotNull(result.Members);
            var expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, result.Members.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name, typeof(D.Member).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMember_MemberIsAddedToTheTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, MemberName, false);

            // Verify
            var expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, team.Members.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMemberAndEstimationStarted_ScrumMasterIsEstimationParticipant()
        {
            // Arrange
            var team = CreateBasicTeam();
            team.ScrumMaster.StartEstimation();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, MemberName, false);

            // Verify
            var expectedParticipants = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedParticipants, team.EstimationParticipants.Select(m => m.MemberName).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndObserverNameAsObserver_ReturnsTeamJoined()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.JoinTeam(TeamName, ObserverName, true).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.IsNotNull(result.Observers);
            var expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, result.Observers.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.Observer).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.Observers.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndObserverNameAsObserver_ObserverIsAddedToTheTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, ObserverName, true);

            // Verify
            var expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, team.Observers.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.JoinTeam(null, MemberName, false));
        }

        [TestMethod]
        public void JoinTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.JoinTeam(TeamName, null, false));
        }

        [TestMethod]
        public void JoinTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.JoinTeam(LongTeamName, MemberName, false));
        }

        [TestMethod]
        public void JoinTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.JoinTeam(TeamName, LongMemberName, false));
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndScrumMasterName_ReturnsReconnectedTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, ScrumMasterName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
            Assert.IsNotNull(result.ScrumTeam.Members);
            var expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberName_ReturnsReconnectedTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
            Assert.IsNotNull(result.ScrumTeam.Members);
            var expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name, typeof(D.Member).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndObserverName_ReturnsReconnectedTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var observer = team.Join(ObserverName, true);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, ObserverName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);

            Assert.IsNotNull(result.ScrumTeam.Members);
            var expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());

            Assert.IsNotNull(result.ScrumTeam.Observers);
            var expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, result.ScrumTeam.Observers.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndDisconnectedScrumMasterName_ReturnsReconnectedTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            team.Disconnect(ScrumMasterName);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, ScrumMasterName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(3, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
            Assert.IsNotNull(result.ScrumTeam.Members);
            var expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            var expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name, typeof(D.Member).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());

            Assert.IsFalse(team.ScrumMaster.IsDormant);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndNonExistingMemberName_ArgumentException()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName);

            // Verify
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameWithMessages_ReturnsLastMessageId()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(1, result.LastMessageId);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimationInProgress_NoEstimationResult()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();
            member.Estimation = new D.Estimation(1);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNull(result.ScrumTeam.EstimationResult);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimationFinished_EstimationResultIsSet()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();
            member.Estimation = new D.Estimation(1);
            team.ScrumMaster.Estimation = new D.Estimation(2);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNotNull(result.ScrumTeam.EstimationResult);

            var expectedEstimations = new double?[] { 2, 1 };
            CollectionAssert.AreEquivalent(expectedEstimations, result.ScrumTeam.EstimationResult.Select(e => e.Estimation.Value).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndMemberEstimated_ReturnsSelectedEstimation()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();
            member.Estimation = new D.Estimation(1);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.AreEqual<double?>(1, result.SelectedEstimation.Value);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndMemberNotEstimated_NoSelectedEstimation()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNull(result.SelectedEstimation);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimationNotStarted_NoSelectedEstimation()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNull(result.SelectedEstimation);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimationStarted_AllMembersAreEstimationParticipants()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimation();

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNotNull(result.ScrumTeam.EstimationParticipants);
            var expectedParticipants = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEqual(expectedParticipants, result.ScrumTeam.EstimationParticipants.Select(p => p.MemberName).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimationNotStarted_NoEstimationParticipants()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNull(result.ScrumTeam.EstimationParticipants);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.ReconnectTeam(null, MemberName));
        }

        [TestMethod]
        public void ReconnectTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.ReconnectTeam(TeamName, null));
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.ReconnectTeam(LongTeamName, MemberName));
        }

        [TestMethod]
        public void ReconnectTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.ReconnectTeam(TeamName, LongMemberName));
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndScrumMasterName_ScrumMasterIsDormant()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, ScrumMasterName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsTrue(team.ScrumMaster.IsDormant);
            var expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, team.Members.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndMemberName_MemberIsRemovedFromTheTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, MemberName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            var expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, team.Members.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndObserverName_ObserverIsRemovedFromTheTeam()
        {
            // Arrange
            var team = CreateBasicTeam();
            team.Join(ObserverName, true);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, ObserverName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsFalse(team.Observers.Any());
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.DisconnectTeam(null, MemberName));
        }

        [TestMethod]
        public void DisconnectTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.DisconnectTeam(TeamName, null));
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.DisconnectTeam(LongTeamName, MemberName));
        }

        [TestMethod]
        public void DisconnectTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.DisconnectTeam(TeamName, LongMemberName));
        }

        [TestMethod]
        public void StartEstimation_TeamName_ScrumTeamEstimationIsInProgress()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimation(TeamName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.AreEqual<D.TeamState>(D.TeamState.EstimationInProgress, team.State);
        }

        [TestMethod]
        public void StartEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.StartEstimation(null));
        }

        [TestMethod]
        public void StartEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.StartEstimation(LongTeamName));
        }

        [TestMethod]
        public void CancelEstimation_TeamName_ScrumTeamEstimationIsCanceled()
        {
            // Arrange
            var team = CreateBasicTeam();
            team.ScrumMaster.StartEstimation();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimation(TeamName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.AreEqual<D.TeamState>(D.TeamState.EstimationCanceled, team.State);
        }

        [TestMethod]
        public void CancelEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.CancelEstimation(null));
        }

        [TestMethod]
        public void CancelEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.CancelEstimation(LongTeamName));
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndScrumMasterName_EstimationIsSetForScrumMaster()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, ScrumMasterName, 2.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(team.ScrumMaster.Estimation);
            Assert.AreEqual<double?>(2.0, team.ScrumMaster.Estimation.Value);
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndScrumMasterNameAndMinus1111111_EstimationIsSetToNull()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, ScrumMasterName, -1111111.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(team.ScrumMaster.Estimation);
            Assert.IsNull(team.ScrumMaster.Estimation.Value);
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndMemberNameAndMinus1111100_EstimationOfMemberIsSetToInfinity()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, MemberName, -1111100.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimation);
            Assert.IsTrue(double.IsPositiveInfinity(member.Estimation.Value.Value));
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndMemberName_EstimationOfMemberIsSet()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, MemberName, 8.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimation);
            Assert.AreEqual<double?>(8.0, member.Estimation.Value);
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndMemberNameAndMinus1111100_EstimationOfMemberIsSetInifinty()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, MemberName, -1111100.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimation);
            Assert.IsTrue(double.IsPositiveInfinity(member.Estimation.Value.Value));
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameAndMemberNameAndMinus1111111_EstimationIsSetToNull()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, MemberName, -1111111.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimation);
            Assert.IsNull(member.Estimation.Value);
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.SubmitEstimation(null, MemberName, 0.0));
        }

        [TestMethod]
        public void SubmitEstimation_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => target.SubmitEstimation(TeamName, null, 0.0));
        }

        [TestMethod]
        public void SubmitEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.SubmitEstimation(LongTeamName, MemberName, 1.0));
        }

        [TestMethod]
        public void SubmitEstimation_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            Assert.ThrowsException<ArgumentException>(() => target.SubmitEstimation(TeamName, LongMemberName, 1.0));
        }

        [TestMethod]
        public async Task GetMessages_MemberJoinedTeam_ScrumMasterGetsMessage()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => team.ScrumMaster.Messages.ToList()).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = await target.GetMessages(TeamName, ScrumMasterName, 0, default(CancellationToken));

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(1, result.Count);
            Assert.AreEqual<long>(1, result[0].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, result[0].Type);
            Assert.IsInstanceOfType(result[0], typeof(MemberMessage));
            var memberMessage = (MemberMessage)result[0];
            Assert.IsNotNull(memberMessage.Member);
            Assert.AreEqual<string>(MemberName, memberMessage.Member.Name);
        }

        [TestMethod]
        public async Task GetMessages_EstimationEnded_ScrumMasterGetsMessages()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = (D.Member)team.Join(MemberName, false);
            var master = team.ScrumMaster;
            master.StartEstimation();
            master.Estimation = new D.Estimation(1.0);
            member.Estimation = new D.Estimation(2.0);

            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(master, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => master.Messages.ToList()).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = await target.GetMessages(TeamName, ScrumMasterName, 1, default(CancellationToken));

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(4, result.Count);
            Assert.AreEqual<long>(2, result[0].Id);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, result[0].Type);

            Assert.AreEqual<long>(3, result[1].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[1].Type);
            Assert.AreEqual<long>(4, result[2].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[2].Type);

            Assert.AreEqual<long>(5, result[3].Id);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, result[3].Type);
            Assert.IsInstanceOfType(result[3], typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)result[3];

            Assert.IsNotNull(estimationResultMessage.EstimationResult);
            var expectedResult = new Tuple<string, double>[]
            {
                new Tuple<string, double>(ScrumMasterName, 1.0),
                new Tuple<string, double>(MemberName, 2.0)
            };
            CollectionAssert.AreEquivalent(expectedResult, estimationResultMessage.EstimationResult.Select(i => new Tuple<string, double>(i.Member.Name, i.Estimation.Value.Value)).ToList());
        }

        [TestMethod]
        public async Task GetMessages_ScrumMasterDisconnected_MemberGetsEmptyMessage()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            team.Disconnect(ScrumMasterName);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(member, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => member.Messages.ToList()).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = await target.GetMessages(TeamName, MemberName, 0, default(CancellationToken));

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(1, result.Count);
            Assert.AreEqual<long>(1, result[0].Id);
            Assert.AreEqual<MessageType>(MessageType.Empty, result[0].Type);
        }

        [TestMethod]
        public async Task GetMessages_MemberDisconnected_ScrumMasterGetsMessage()
        {
            // Arrange
            var team = CreateBasicTeam();
            team.Join(MemberName, false);
            team.Disconnect(MemberName);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => team.ScrumMaster.Messages.ToList()).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = await target.GetMessages(TeamName, ScrumMasterName, 0, default(CancellationToken));

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(2, result.Count);
            Assert.AreEqual<long>(2, result[1].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, result[1].Type);
            Assert.IsInstanceOfType(result[1], typeof(MemberMessage));
            var memberMessage = (MemberMessage)result[1];
            Assert.IsNotNull(memberMessage.Member);
            Assert.AreEqual<string>(MemberName, memberMessage.Member.Name);
        }

        [TestMethod]
        public async Task GetMessages_NoMessagesOnTime_ReturnsEmptyCollection()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<D.Message>()).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = await target.GetMessages(TeamName, ScrumMasterName, 0, default(CancellationToken));

            // Verify
            planningPoker.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(0, result.Count);
        }

        [TestMethod]
        public async Task GetMessages_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.GetMessages(null, MemberName, 0, default(CancellationToken)));
        }

        [TestMethod]
        public async Task GetMessages_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.GetMessages(TeamName, null, 0, default(CancellationToken)));
        }

        [TestMethod]
        public async Task GetMessages_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.GetMessages(LongTeamName, MemberName, 0, default(CancellationToken)));
        }

        [TestMethod]
        public async Task GetMessages_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.GetMessages(TeamName, LongMemberName, 0, default(CancellationToken)));
        }

        private static D.ScrumTeam CreateBasicTeam()
        {
            var result = new D.ScrumTeam(TeamName);
            result.SetScrumMaster(ScrumMasterName);
            return result;
        }

        private static Mock<D.IScrumTeamLock> CreateTeamLock(D.ScrumTeam scrumTeam)
        {
            var result = new Mock<D.IScrumTeamLock>(MockBehavior.Strict);
            result.Setup(l => l.Team).Returns(scrumTeam);
            result.Setup(l => l.Lock()).Verifiable();
            result.Setup(l => l.Dispose()).Verifiable();
            return result;
        }
    }
}
