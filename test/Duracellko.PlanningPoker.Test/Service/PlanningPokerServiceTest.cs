using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Domain.Test;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Test.Service;

[TestClass]
public class PlanningPokerServiceTest
{
    private const string TeamName = "test team";
    private const string ScrumMasterName = "master";
    private const string MemberName = "member";
    private const string ObserverName = "observer";
    private const string NoVoteName = "no vote";

    private const string LongTeamName = "ttttttttttttttttttttttttttttttttttttttttttttttttttt";
    private const string LongMemberName = "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm";

    [TestMethod]
    public void Constructor_PlanningPoker_PlanningPokerPropertyIsSet()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);

        // Act
        var result = CreatePlanningPokerService(planningPoker.Object);

        // Verify
        Assert.AreEqual<D.IPlanningPoker>(planningPoker.Object, result.PlanningPoker);
    }

    [TestMethod]
    public void Constructor_Null_ArgumentNullException()
    {
        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => CreatePlanningPokerService(null!));
    }

    [TestMethod]
    [DataRow(Deck.Standard, D.Deck.Standard)]
    [DataRow(Deck.Fibonacci, D.Deck.Fibonacci)]
    [DataRow(Deck.RockPaperScissorsLizardSpock, D.Deck.RockPaperScissorsLizardSpock)]
    public void CreateTeam_TeamNameAndScrumMasterName_ReturnsCreatedTeam(Deck deck, D.Deck domainDeck)
    {
        // Arrange
        var guid = Guid.NewGuid();
        var team = CreateBasicTeam(guidProvider: new GuidProviderMock(guid));
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName, domainDeck))
            .Returns(teamLock.Object).Verifiable();

        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.CreateTeam(TeamName, ScrumMasterName, deck).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual<string>(TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual<string>(nameof(D.ScrumMaster), resultTeam.ScrumMaster.Type);
    }

    [TestMethod]
    public void CreateTeam_TeamNameAndScrumMasterName_ReturnsTeamWithAvilableEstimations()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName, D.Deck.Standard))
            .Returns(teamLock.Object).Verifiable();

        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.CreateTeam(TeamName, ScrumMasterName, Deck.Standard).Value;

        // Verify
        Assert.IsNotNull(result);
        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam?.AvailableEstimations);
        var expectedCollection = new double?[]
        {
            0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, EstimationTestData.Infinity, EstimationTestData.Unknown
        };
        CollectionAssert.AreEquivalent(expectedCollection, resultTeam.AvailableEstimations.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void CreateTeam_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CreateTeam(null!, ScrumMasterName, Deck.Standard));
    }

    [TestMethod]
    public void CreateTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CreateTeam(TeamName, null!, Deck.Standard));
    }

    [TestMethod]
    public void CreateTeam_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CreateTeam(LongTeamName, ScrumMasterName, Deck.Standard));
    }

    [TestMethod]
    public void CreateTeam_ScrumMasterNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CreateTeam(TeamName, LongMemberName, Deck.Standard));
    }

    [TestMethod]
    public void CreateTeam_TeamAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var planningPokerException = new D.PlanningPokerException(ErrorCodes.ScrumTeamAlreadyExists, TeamName);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName, D.Deck.Standard))
            .Throws(planningPokerException).Verifiable();

        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.CreateTeam(TeamName, ScrumMasterName, Deck.Standard);

        // Verify
        planningPoker.Verify();
        var expectedValue = @"PlanningPokerException:{""Error"":""ScrumTeamAlreadyExists"",""Message"":""Cannot create Scrum Team \u0027test team\u0027. Team with that name already exists."",""Argument"":""test team""}";
        VerifyBadRequest(result, expectedValue);
    }

    [TestMethod]
    public void JoinTeam_TeamNameAndMemberNameAsMember_ReturnsTeamJoined()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc));
        var team = CreateBasicTeam(guidProvider: guidProvider, dateTimeProvider: dateTimeProvider);
        team.ScrumMaster!.StartTimer(TimeSpan.FromSeconds(122));
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.JoinTeam(TeamName, MemberName, false).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual<string>(TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(new DateTime(2021, 11, 17, 9, 0, 3, DateTimeKind.Utc), resultTeam.TimerEndTime);
        Assert.IsNotNull(resultTeam.Members);
        var expectedMembers = new string[] { ScrumMasterName, MemberName };
        CollectionAssert.AreEquivalent(expectedMembers, resultTeam.Members.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.ScrumMaster), nameof(D.Member) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, resultTeam.Members.Select(m => m.Type).ToList());
    }

    [TestMethod]
    public void JoinTeam_TeamNameAndMemberNameAsMember_MemberIsAddedToTheTeam()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.ScrumMaster!.StartEstimation();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.JoinTeam(TeamName, MemberName, false);

        // Verify
        var expectedParticipants = new string[] { ScrumMasterName };
        Assert.IsNotNull(team.EstimationParticipants);
        CollectionAssert.AreEquivalent(expectedParticipants, team.EstimationParticipants.Select(m => m.MemberName).ToList());
    }

    [TestMethod]
    public void JoinTeam_TeamNameAndObserverNameAsObserver_ReturnsTeamJoined()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider: guidProvider);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.JoinTeam(TeamName, ObserverName, true).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual<string>(TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.IsNull(resultTeam.TimerEndTime);
        Assert.IsNotNull(resultTeam.Observers);
        var expectedObservers = new string[] { ObserverName };
        CollectionAssert.AreEquivalent(expectedObservers, resultTeam.Observers.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.Observer) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, resultTeam.Observers.Select(m => m.Type).ToList());
    }

    [TestMethod]
    public void JoinTeam_TeamNameAndObserverNameAsObserver_ObserverIsAddedToTheTeam()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.JoinTeam(null!, MemberName, false));
    }

    [TestMethod]
    public void JoinTeam_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.JoinTeam(TeamName, null!, false));
    }

    [TestMethod]
    public void JoinTeam_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.JoinTeam(LongTeamName, MemberName, false));
    }

    [TestMethod]
    public void JoinTeam_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.JoinTeam(TeamName, LongMemberName, false));
    }

    [TestMethod]
    public void JoinTeam_MemberAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var team = CreateBasicTeam();
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.JoinTeam(TeamName, MemberName, false);

        // Verify
        planningPoker.Verify();
        var expectedValue = @"PlanningPokerException:{""Error"":""MemberAlreadyExists"",""Message"":""Member or observer named \u0027member\u0027 already exists in the team."",""Argument"":""member""}";
        VerifyBadRequest(result, expectedValue);
    }

    [TestMethod]
    public void JoinTeam_TeamDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var planningPokerException = new D.PlanningPokerException(ErrorCodes.ScrumTeamNotExist, TeamName);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Throws(planningPokerException).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.JoinTeam(TeamName, MemberName, false);

        // Verify
        planningPoker.Verify();
        var expectedValue = @"PlanningPokerException:{""Error"":""ScrumTeamNotExist"",""Message"":""Scrum Team \u0027test team\u0027 does not exist."",""Argument"":""test team""}";
        VerifyBadRequest(result, expectedValue);
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndScrumMasterName_ReturnsReconnectedTeam()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider: guidProvider);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.ReconnectTeam(TeamName, ScrumMasterName).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual<long>(0, result.LastMessageId);
        Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
        Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
        Assert.IsNull(result.ScrumTeam.TimerEndTime);
        Assert.IsNotNull(result.ScrumTeam.Members);
        var expectedMembers = new string[] { ScrumMasterName };
        CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.ScrumMaster) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndMemberName_ReturnsReconnectedTeam()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider: guidProvider);
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.ReconnectTeam(TeamName, MemberName).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual<long>(0, result.LastMessageId);
        Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
        Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
        Assert.IsNull(result.ScrumTeam.TimerEndTime);
        Assert.IsNotNull(result.ScrumTeam.Members);
        var expectedMembers = new string[] { ScrumMasterName, MemberName };
        CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.ScrumMaster), nameof(D.Member) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndObserverName_ReturnsReconnectedTeam()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc));
        var team = CreateBasicTeam(guidProvider: guidProvider, dateTimeProvider: dateTimeProvider);
        team.ScrumMaster!.StartTimer(TimeSpan.FromSeconds(122));
        team.Join(ObserverName, true);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.ReconnectTeam(TeamName, ObserverName).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual<long>(0, result.LastMessageId);
        Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
        Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
        Assert.AreEqual(new DateTime(2021, 11, 17, 9, 0, 3, DateTimeKind.Utc), result.ScrumTeam.TimerEndTime);

        Assert.IsNotNull(result.ScrumTeam.Members);
        var expectedMembers = new string[] { ScrumMasterName };
        CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.ScrumMaster) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());

        Assert.IsNotNull(result.ScrumTeam.Observers);
        var expectedObservers = new string[] { ObserverName };
        CollectionAssert.AreEquivalent(expectedObservers, result.ScrumTeam.Observers.Select(m => m.Name).ToList());
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndDisconnectedScrumMasterName_ReturnsReconnectedTeam()
    {
        // Arrange
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider: guidProvider);
        team.Join(MemberName, false);
        team.Disconnect(ScrumMasterName);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.ReconnectTeam(TeamName, ScrumMasterName).Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(result);
        Assert.AreEqual<Guid>(guid, result.SessionId);
        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual<long>(3, result.LastMessageId);
        Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
        Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
        Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
        Assert.IsNotNull(result.ScrumTeam.Members);
        var expectedMembers = new string[] { ScrumMasterName, MemberName };
        CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
        var expectedMemberTypes = new string[] { nameof(D.ScrumMaster), nameof(D.Member) };
        CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());

        Assert.IsFalse(team.ScrumMaster!.IsDormant);
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndNonExistingMemberName_ArgumentException()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.ReconnectTeam(TeamName, MemberName);

        // Verify
        Assert.IsInstanceOfType<BadRequestObjectResult>(result.Result);
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndMemberNameWithMessages_ReturnsLastMessageId()
    {
        // Arrange
        var team = CreateBasicTeam();
        var member = team.Join(MemberName, false);
        team.ScrumMaster!.StartEstimation();

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.ScrumMaster!.StartEstimation();
        member.Estimation = new D.Estimation(1);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.ScrumMaster!.StartEstimation();
        member.Estimation = new D.Estimation(1);
        team.ScrumMaster.Estimation = new D.Estimation(2);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        CollectionAssert.AreEquivalent(expectedEstimations, result.ScrumTeam.EstimationResult.Select(e => e.Estimation!.Value).ToList());
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameAndMemberNameAndMemberEstimated_ReturnsSelectedEstimation()
    {
        // Arrange
        var team = CreateBasicTeam();
        var member = (D.Member)team.Join(MemberName, false);
        team.ScrumMaster!.StartEstimation();
        member.Estimation = new D.Estimation(1);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.Join(MemberName, false);
        team.ScrumMaster!.StartEstimation();

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.Join(MemberName, false);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.Join(MemberName, false);
        team.ScrumMaster!.StartEstimation();

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        team.Join(MemberName, false);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
    public void ReconnectTeam_TeamNameAndScrumMasterName_UpdatedLastActivityOfScrumMaster()
    {
        // Arrange
        var utcNow = new DateTime(2020, 6, 5, 22, 14, 31, DateTimeKind.Utc);
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(utcNow);
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider: guidProvider, dateTimeProvider: dateTimeProvider);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        utcNow = utcNow.AddMinutes(2.2);
        dateTimeProvider.SetUtcNow(utcNow);
        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        target.ReconnectTeam(TeamName, ScrumMasterName);

        // Verify
        Assert.AreEqual<DateTime>(utcNow, team.ScrumMaster!.LastActivity);
        Assert.AreEqual<Guid>(guid, team.ScrumMaster.SessionId);
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.ReconnectTeam(null!, MemberName));
    }

    [TestMethod]
    public void ReconnectTeam_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.ReconnectTeam(TeamName, null!));
    }

    [TestMethod]
    public void ReconnectTeam_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.ReconnectTeam(LongTeamName, MemberName));
    }

    [TestMethod]
    public void ReconnectTeam_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.ReconnectTeam(TeamName, LongMemberName));
    }

    [TestMethod]
    public void ReconnectTeam_TeamDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var planningPokerException = new D.PlanningPokerException(ErrorCodes.ScrumTeamNotExist, TeamName);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Throws(planningPokerException).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var result = target.ReconnectTeam(TeamName, MemberName);

        // Verify
        planningPoker.Verify();
        var expectedValue = @"PlanningPokerException:{""Error"":""ScrumTeamNotExist"",""Message"":""Scrum Team \u0027test team\u0027 does not exist."",""Argument"":""test team""}";
        VerifyBadRequest(result, expectedValue);
    }

    [TestMethod]
    public void DisconnectTeam_TeamNameAndScrumMasterName_ScrumMasterIsDormant()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.DisconnectTeam(TeamName, ScrumMasterName);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsTrue(team.ScrumMaster!.IsDormant);
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
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.DisconnectTeam(null!, MemberName));
    }

    [TestMethod]
    public void DisconnectTeam_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.DisconnectTeam(TeamName, null!));
    }

    [TestMethod]
    public void DisconnectTeam_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.DisconnectTeam(LongTeamName, MemberName));
    }

    [TestMethod]
    public void DisconnectTeam_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.DisconnectTeam(TeamName, LongMemberName));
    }

    [TestMethod]
    public void StartEstimation_TeamName_ScrumTeamEstimationIsInProgress()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.StartEstimation(null!));
    }

    [TestMethod]
    public void StartEstimation_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.StartEstimation(LongTeamName));
    }

    [TestMethod]
    public void CancelEstimation_TeamName_ScrumTeamEstimationIsCanceled()
    {
        // Arrange
        var team = CreateBasicTeam();
        team.ScrumMaster!.StartEstimation();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CancelEstimation(null!));
    }

    [TestMethod]
    public void CancelEstimation_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CancelEstimation(LongTeamName));
    }

    [TestMethod]
    public void CloseEstimation_TeamName_ScrumTeamEstimationIsFinished()
    {
        // Arrange
        var team = CreateBasicTeam();
        team.ScrumMaster!.StartEstimation();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.CloseEstimation(TeamName);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.AreEqual<D.TeamState>(D.TeamState.EstimationFinished, team.State);
    }

    [TestMethod]
    public void CloseEstimation_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CloseEstimation(null!));
    }

    [TestMethod]
    public void CloseEstimation_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CloseEstimation(LongTeamName));
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndScrumMasterName_EstimationIsSetForScrumMaster()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, ScrumMasterName, 2.0);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(team.ScrumMaster!.Estimation);
        Assert.AreEqual<double?>(2.0, team.ScrumMaster.Estimation.Value);
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndScrumMasterNameAndUnknown_EstimationIsSetToUnknown()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, ScrumMasterName, EstimationTestData.Unknown);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(team.ScrumMaster!.Estimation);
        Assert.IsNotNull(team.ScrumMaster.Estimation.Value);
        Assert.AreEqual<double?>(EstimationTestData.Unknown, team.ScrumMaster.Estimation.Value);
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndScrumMasterNameAndEstimationInfinity_EstimationOfMemberIsSetToInfinity()
    {
        // Arrange
        var team = CreateBasicTeam();
        var scrumMaster = team.ScrumMaster!;
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, ScrumMasterName, EstimationTestData.Infinity);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(scrumMaster.Estimation);
        Assert.IsNotNull(scrumMaster.Estimation.Value);
        Assert.AreEqual<double?>(EstimationTestData.Infinity, scrumMaster.Estimation.Value);
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndScrumMasterNameAndNoVote_EstimationOfMemberIsSetToNull()
    {
        // Arrange
        var team = CreateBasicTeam();
        var scrumMaster = team.ScrumMaster!;
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, ScrumMasterName, null);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(scrumMaster.Estimation);
        Assert.IsNull(scrumMaster.Estimation.Value);
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
        var target = CreatePlanningPokerService(planningPoker.Object);

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
    public void SubmitEstimation_TeamNameAndMemberNameAndEstimationInfinity_EstimationOfMemberIsSetInifinty()
    {
        // Arrange
        var team = CreateBasicTeam();
        var member = (D.Member)team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, MemberName, EstimationTestData.Infinity);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(member.Estimation);
        Assert.IsNotNull(member.Estimation.Value);
        Assert.AreEqual<double?>(EstimationTestData.Infinity, member.Estimation.Value);
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndMemberNameAndUnknown_EstimationIsSetToUnknown()
    {
        // Arrange
        var team = CreateBasicTeam();
        var member = (D.Member)team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, MemberName, EstimationTestData.Unknown);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNotNull(member.Estimation);
        Assert.IsNotNull(member.Estimation.Value);
        Assert.AreEqual<double?>(EstimationTestData.Unknown, member.Estimation.Value);
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameAndMemberNameAndNoVote_EstimationIsSetToNull()
    {
        // Arrange
        var team = CreateBasicTeam();
        var member = (D.Member)team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.SubmitEstimation(TeamName, MemberName, null);

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
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.SubmitEstimation(null!, MemberName, 0.0));
    }

    [TestMethod]
    public void SubmitEstimation_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.SubmitEstimation(TeamName, null!, 0.0));
    }

    [TestMethod]
    public void SubmitEstimation_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.SubmitEstimation(LongTeamName, MemberName, 1.0));
    }

    [TestMethod]
    public void SubmitEstimation_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.SubmitEstimation(TeamName, LongMemberName, 1.0));
    }

    [TestMethod]
    public void ChangeDeck_TeamNameAndFibonacci_AvailableEstimationsAreChanged()
    {
        // Arrange
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.ChangeDeck(TeamName, Deck.Fibonacci);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        var fibonacciDeck = D.DeckProvider.Default.GetDeck(D.Deck.Fibonacci);
        Assert.AreEqual(fibonacciDeck, team.AvailableEstimations);
    }

    [TestMethod]
    public void ChangeDeck_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.ChangeDeck(string.Empty, Deck.Fibonacci));
    }

    [TestMethod]
    public void ChangeDeck_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.ChangeDeck(LongTeamName, Deck.Standard));
    }

    [TestMethod]
    public void StartTimer_Duration_ScrumTeamTimerIsSet()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.StartTimer(TeamName, MemberName, 122);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.AreEqual(new DateTime(2021, 11, 17, 9, 0, 3, DateTimeKind.Utc), team.TimerEndTime);
    }

    [TestMethod]
    public void StartTimer_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.StartTimer(null!, MemberName, 122));
    }

    [TestMethod]
    public void StartTimer_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.StartTimer(LongTeamName, MemberName, 122));
    }

    [TestMethod]
    public void StartTimer_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.StartTimer(TeamName, null!, 122));
    }

    [TestMethod]
    public void StartTimer_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.StartTimer(TeamName, LongMemberName, 122));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-122)]
    public void StartTimer_NegativeDuration_ArgumentOutOfRangeException(int duration)
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => target.StartTimer(TeamName, MemberName, duration));
    }

    [TestMethod]
    public void CancelTimer_MemberName_ScrumTeamTimerIsSet()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        team.Join(MemberName, false);
        team.ScrumMaster!.StartTimer(TimeSpan.FromSeconds(122));
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        target.CancelTimer(TeamName, MemberName);

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        teamLock.Verify(l => l.Team);

        Assert.IsNull(team.TimerEndTime);
    }

    [TestMethod]
    public void CancelTimer_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CancelTimer(null!, MemberName));
    }

    [TestMethod]
    public void CancelTimer_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CancelTimer(LongTeamName, MemberName));
    }

    [TestMethod]
    public void CancelTimer_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => target.CancelTimer(TeamName, null!));
    }

    [TestMethod]
    public void CancelTimer_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        Assert.ThrowsExactly<ArgumentException>(() => target.CancelTimer(TeamName, LongMemberName));
    }

    [TestMethod]
    public async Task GetMessages_MemberJoinedTeam_ScrumMasterGetsMessage()
    {
        // Arrange
        var team = CreateBasicTeam(new GuidProviderMock());
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => team.ScrumMaster!.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, ScrumMasterName, GuidProviderMock.DefaultGuid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual<long>(1, result[0].Id);
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, result[0].Type);
        var memberMessage = Assert.IsInstanceOfType<MemberMessage>(result[0]);
        Assert.IsNotNull(memberMessage.Member);
        Assert.AreEqual<string>(MemberName, memberMessage.Member.Name);

        Assert.AreEqual(1, team.ScrumMaster!.Messages.Count());
        Assert.AreEqual(0, team.ScrumMaster.AcknowledgedMessageId);
    }

    [TestMethod]
    public async Task GetMessages_EstimationEnded_ScrumMasterGetsMessages()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var team = CreateBasicTeam(new GuidProviderMock(guid));
        var member = (D.Member)team.Join(MemberName, false);
        var noVote = (D.Member)team.Join(NoVoteName, false);
        var master = team.ScrumMaster!;
        master.StartEstimation();
        master.Estimation = new D.Estimation(EstimationTestData.Infinity);
        member.Estimation = new D.Estimation(2.0);
        noVote.Estimation = new D.Estimation();

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(master, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => master.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, ScrumMasterName, guid, 2, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(5, result);
        Assert.AreEqual<long>(3, result[0].Id);
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, result[0].Type);

        Assert.AreEqual<long>(4, result[1].Id);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[1].Type);
        Assert.AreEqual<long>(5, result[2].Id);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[2].Type);
        Assert.AreEqual<long>(6, result[3].Id);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[3].Type);

        Assert.AreEqual<long>(7, result[4].Id);
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, result[4].Type);
        var estimationResultMessage = Assert.IsInstanceOfType<EstimationResultMessage>(result[4]);

        Assert.IsNotNull(estimationResultMessage.EstimationResult);
        var expectedResult = new Tuple<string, double?>[]
        {
            new(ScrumMasterName, EstimationTestData.Infinity),
            new(MemberName, 2.0),
            new(NoVoteName, null)
        };
        CollectionAssert.AreEquivalent(expectedResult, estimationResultMessage.EstimationResult.Select(i => new Tuple<string, double?>(i.Member!.Name, i.Estimation!.Value)).ToList());

        Assert.AreEqual(5, team.ScrumMaster!.Messages.Count());
        Assert.AreEqual(2, team.ScrumMaster.AcknowledgedMessageId);
    }

    [TestMethod]
    public async Task GetMessages_AvailableEstimationsChanged_MemberGetsMessages()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var team = CreateBasicTeam(new GuidProviderMock(guid));
        var member = (D.Member)team.Join(MemberName, false);
        var deck = D.DeckProvider.Default.GetDeck(D.Deck.Fibonacci);
        team.ChangeAvailableEstimations(deck);

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => member.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, MemberName, guid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual<long>(1, result[0].Id);
        Assert.AreEqual<MessageType>(MessageType.AvailableEstimationsChanged, result[0].Type);
        var estimationSetMessage = Assert.IsInstanceOfType<EstimationSetMessage>(result[0]);

        var estimations = estimationSetMessage.Estimations;
        Assert.IsNotNull(estimations);
        Assert.HasCount(13, estimations);
        Assert.AreEqual(0, estimations[0].Value);
        Assert.AreEqual(1, estimations[1].Value);
        Assert.AreEqual(2, estimations[2].Value);
        Assert.AreEqual(3, estimations[3].Value);
        Assert.AreEqual(5, estimations[4].Value);
        Assert.AreEqual(8, estimations[5].Value);
        Assert.AreEqual(13, estimations[6].Value);
        Assert.AreEqual(21, estimations[7].Value);
        Assert.AreEqual(34, estimations[8].Value);
        Assert.AreEqual(55, estimations[9].Value);
        Assert.AreEqual(89, estimations[10].Value);
        Assert.AreEqual(EstimationTestData.Infinity, estimations[11].Value);
        Assert.AreEqual(EstimationTestData.Unknown, estimations[12].Value);
    }

    [TestMethod]
    public async Task GetMessages_ScrumMasterDisconnected_MemberGetsEmptyMessage()
    {
        // Arrange
        var team = CreateBasicTeam(new GuidProviderMock());
        var member = team.Join(MemberName, false);
        team.Disconnect(ScrumMasterName);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => member.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, MemberName, GuidProviderMock.DefaultGuid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual<long>(1, result[0].Id);
        Assert.AreEqual<MessageType>(MessageType.Empty, result[0].Type);
    }

    [TestMethod]
    public async Task GetMessages_MemberDisconnected_ScrumMasterGetsMessage()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var team = CreateBasicTeam(new GuidProviderMock(guid));
        team.Join(MemberName, false);
        team.Disconnect(MemberName);
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => team.ScrumMaster!.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, ScrumMasterName, guid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreEqual<long>(2, result[1].Id);
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, result[1].Type);
        var memberMessage = Assert.IsInstanceOfType<MemberMessage>(result[1]);
        Assert.IsNotNull(memberMessage.Member);
        Assert.AreEqual<string>(MemberName, memberMessage.Member.Name);
    }

    [TestMethod]
    public async Task GetMessages_TimerStarted_MemberGetsMessage()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc));
        var team = CreateBasicTeam(new GuidProviderMock(guid), dateTimeProvider: dateTimeProvider);
        var member = team.Join(MemberName, false);
        team.ScrumMaster!.StartTimer(TimeSpan.FromSeconds(122));
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(member, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => member.Messages.ToList()).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, MemberName, guid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.AreEqual<long>(1, result[0].Id);
        Assert.AreEqual<MessageType>(MessageType.TimerStarted, result[0].Type);
        var timerMessage = Assert.IsInstanceOfType<TimerMessage>(result[0]);
        Assert.AreEqual(new DateTime(2021, 11, 17, 9, 0, 3, DateTimeKind.Utc), timerMessage.EndTime);
    }

    [TestMethod]
    public async Task GetMessages_NoMessagesOnTime_ReturnsEmptyCollection()
    {
        // Arrange
        var team = CreateBasicTeam(new GuidProviderMock());
        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster!, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, ScrumMasterName, GuidProviderMock.DefaultGuid, 0, default);
        var result = resultAction.Value;

        // Verify
        planningPoker.Verify();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetMessages_InvalidScrumMasterSessionId_NotFoundResponse()
    {
        // Arrange
        var team = CreateBasicTeam(new GuidProviderMock());
        team.Join(MemberName, false);
        var teamLock = CreateTeamLock(team);
        var messageCount = team.ScrumMaster!.Messages.Count();

        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, ScrumMasterName, Guid.NewGuid(), 0, default);
        var result = resultAction.Result;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        Assert.IsNotNull(result);
        var notFoundObjectResult = Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        var resultValue = notFoundObjectResult.Value?.ToString();
        Assert.IsNotNull(resultValue);
        Assert.IsTrue(resultValue.Contains("Invalid session ID.", StringComparison.Ordinal));
        Assert.AreEqual(messageCount, team.ScrumMaster.Messages.Count());
    }

    [TestMethod]
    public async Task GetMessages_InvalidMemberSessionId_NotFoundResponse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidProvider = new GuidProviderMock();
        var team = CreateBasicTeam(guidProvider);
        guidProvider.SetGuid(guid);
        var master = team.ScrumMaster!;
        var member = (D.Member)team.Join(MemberName, false);
        master.StartEstimation();
        master.Estimation = new D.Estimation(1.0);
        member.Estimation = new D.Estimation(2.0);
        var messageCount = member.Messages.Count();

        var teamLock = CreateTeamLock(team);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        var resultAction = await target.GetMessages(TeamName, MemberName, GuidProviderMock.DefaultGuid, 1, default);
        var result = resultAction.Result;

        // Verify
        planningPoker.Verify();
        teamLock.Verify();
        Assert.IsNotNull(result);
        var notFoundObjectResult = Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        var resultValue = notFoundObjectResult.Value?.ToString();
        Assert.IsNotNull(resultValue);
        Assert.IsTrue(resultValue.Contains("Invalid session ID.", StringComparison.Ordinal));
        Assert.AreEqual(messageCount, member.Messages.Count());
    }

    [TestMethod]
    public async Task GetMessages_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => target.GetMessages(null!, MemberName, GuidProviderMock.DefaultGuid, 0, default));
    }

    [TestMethod]
    public async Task GetMessages_MemberNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => target.GetMessages(TeamName, null!, GuidProviderMock.DefaultGuid, 0, default));
    }

    [TestMethod]
    public async Task GetMessages_TeamNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.GetMessages(LongTeamName, MemberName, GuidProviderMock.DefaultGuid, 0, default));
    }

    [TestMethod]
    public async Task GetMessages_MemberNameTooLong_ArgumentException()
    {
        // Arrange
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object);

        // Act
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.GetMessages(TeamName, LongMemberName, GuidProviderMock.DefaultGuid, 0, default));
    }

    [TestMethod]
    public void GetCurrentTime_DateTimeProvider_CurrentUtcTime()
    {
        // Arrange
        var utcNow = new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc);
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(utcNow);
        var planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
        var target = CreatePlanningPokerService(planningPoker.Object, dateTimeProvider: dateTimeProvider);

        // Act
        var result = target.GetCurrentTime();

        // Verify
        Assert.AreEqual(utcNow, result.CurrentUtcTime);
    }

    private static D.ScrumTeam CreateBasicTeam(D.GuidProvider? guidProvider = null, D.DateTimeProvider? dateTimeProvider = null)
    {
        var result = new D.ScrumTeam(TeamName, null, dateTimeProvider, guidProvider);
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

    private static PlanningPokerService CreatePlanningPokerService(D.IPlanningPoker planningPoker, D.DateTimeProvider? dateTimeProvider = null)
    {
        dateTimeProvider ??= D.DateTimeProvider.Default;
        return new PlanningPokerService(planningPoker, dateTimeProvider, D.DeckProvider.Default);
    }

    private static void VerifyBadRequest<T>(ActionResult<T> result, string expectedValue)
    {
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType<ObjectResult>(result.Result);
        var objectResult = (ObjectResult)result.Result;
        Assert.AreEqual(400, objectResult.StatusCode);
        Assert.AreEqual(expectedValue, objectResult.Value);
    }
}
