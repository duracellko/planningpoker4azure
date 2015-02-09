// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service.Test
{
    [TestClass]
    public class PlanningPokerServiceTest
    {
        #region Consts

        private const string TeamName = "test team";
        private const string ScrumMasterName = "master";
        private const string MemberName = "member";
        private const string ObserverName = "observer";

        private const string LongTeamName = "ttttttttttttttttttttttttttttttttttttttttttttttttttt";
        private const string LongMemberName = "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm";

        #endregion

        #region Constructor

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Null_ArgumentNullException()
        {
            // Act
            var result = new PlanningPokerService(null);
        }

        #endregion

        #region CreateTeam

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
            var result = target.CreateTeam(TeamName, ScrumMasterName);

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
            var result = target.CreateTeam(TeamName, ScrumMasterName);

            // Verify
            Assert.IsNotNull(result.AvailableEstimations);
            var expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, Estimation.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.AvailableEstimations.Select(e => e.Value).ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(null, ScrumMasterName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(TeamName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(LongTeamName, ScrumMasterName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTeam_ScrumMasterNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(TeamName, LongMemberName);
        }

        #endregion

        #region JoinTeam

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
            var result = target.JoinTeam(TeamName, MemberName, false);

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
        public void JoinTeam_TeamNameAndObserverNameAsObserver_ReturnsTeamJoined()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var result = target.JoinTeam(TeamName, ObserverName, true);

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(null, MemberName, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(LongTeamName, MemberName, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, LongMemberName, false);
        }

        #endregion

        #region DisconnectTeam

        [TestMethod]
        public void DisconnectTeam_TeamNameAndScrumMasterName_ScrumMasterIsRemovedFromTheTeam()
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

            Assert.IsNull(team.ScrumMaster);
            Assert.IsFalse(team.Members.Any());
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void DisconnectTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(null, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DisconnectTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(LongTeamName, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, LongMemberName);
        }

        #endregion

        #region StartEstimation

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StartEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimation(LongTeamName);
        }

        #endregion

        #region CancelEstimation

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void CancelEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CancelEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimation(LongTeamName);
        }

        #endregion

        #region SubmitEstimation

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubmitEstimation_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(null, MemberName, 0.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubmitEstimation_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, null, 0.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubmitEstimation_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(LongTeamName, MemberName, 1.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubmitEstimation_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimation(TeamName, LongMemberName, 1.0);
        }

        #endregion

        #region BeginGetMessages

        [TestMethod]
        public void BeginGetMessages_MemberJoinedTeam_ScrumMasterGetsMessage()
        {
            // Arrange
            var team = CreateBasicTeam();
            var member = team.Join(MemberName, false);
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(true, o)).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var ar = target.BeginGetMessages(TeamName, ScrumMasterName, 0, null, null);
            if (!ar.IsCompleted)
            {
                ar.AsyncWaitHandle.WaitOne(1000);
            }

            var result = target.EndGetMessages(ar);

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
        public void BeginGetMessages_EstimationEnded_ScrumMasterGetsMessages()
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
            planningPoker.Setup(p => p.GetMessagesAsync(master, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(true, o)).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var ar = target.BeginGetMessages(TeamName, ScrumMasterName, 1, null, null);
            if (!ar.IsCompleted)
            {
                Assert.IsTrue(ar.AsyncWaitHandle.WaitOne(1000));
            }

            var result = target.EndGetMessages(ar);

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
        public void BeginGetMessages_NoMessagesOnTime_ReturnsEmptyCollection()
        {
            // Arrange
            var team = CreateBasicTeam();
            var teamLock = CreateTeamLock(team);
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(false, null)).Verifiable();
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            var ar = target.BeginGetMessages(TeamName, ScrumMasterName, 0, null, null);
            if (!ar.IsCompleted)
            {
                ar.AsyncWaitHandle.WaitOne(1000);
            }

            var result = target.EndGetMessages(ar);

            // Verify
            planningPoker.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeginGetMessages_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.BeginGetMessages(null, MemberName, 0, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeginGetMessages_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.BeginGetMessages(TeamName, null, 0, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeginGetMessages_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.BeginGetMessages(LongTeamName, MemberName, 0, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BeginGetMessages_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            var target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.BeginGetMessages(TeamName, LongMemberName, 0, null, null);
        }

        #endregion

        #region EndGetMessages

        #endregion

        #region Private methods

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

        #endregion
    }
}
