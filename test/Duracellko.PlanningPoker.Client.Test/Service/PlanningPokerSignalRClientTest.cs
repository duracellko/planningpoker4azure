using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Service;

[TestClass]
public class PlanningPokerSignalRClientTest
{
    [TestMethod]
    [DataRow(Deck.Standard)]
    [DataRow(Deck.Fibonacci)]
    [DataRow(Deck.RockPaperScissorsLizardSpock)]
    public async Task CreateTeam_TeamNameAndScrumMaster_InvocationMessageIsSent(Deck deck)
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, deck, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("CreateTeam", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, deck };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var scrumTeam = PlanningPokerData.GetScrumTeam();
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task CreateTeam_TeamAndScrumMasterName_ReturnsScrumTeam()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var scrumTeam = PlanningPokerData.GetScrumTeam();
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
    }

    [TestMethod]
    public async Task CreateTeam_TeamNameExists_PlanningPokerException()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, Deck.Standard, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var errorMessage = @"An unexpected error occured. HubException: PlanningPokerException:{""Error"":""ScrumTeamAlreadyExists"",""Message"":""Cannot create Scrum Team \u0027test team\u0027. Team with that name already exists."",""Argument"":""test team""}";
        var returnMessage = new CompletionMessage(invocationId, errorMessage, null, false);
        await fixture.ReceiveMessage(returnMessage);

        var exception = await Assert.ThrowsExactlyAsync<PlanningPokerException>(() => resultTask);

        Assert.AreEqual(ErrorCodes.ScrumTeamAlreadyExists, exception.Error);
        Assert.AreEqual("test team", exception.Argument);
        Assert.AreEqual("Cannot create Scrum Team 'test team'. Team with that name already exists.", exception.Message);
    }

    [TestMethod]
    public async Task JoinTeam_TeamNameAndMemberName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("JoinTeam", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, false };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task JoinTeam_TeamNameAndObserverName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("JoinTeam", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var scrumTeam = PlanningPokerData.GetScrumTeam(observer: true);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeam()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndObserverName_ReturnsScrumTeam()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var scrumTeam = PlanningPokerData.GetScrumTeam(observer: true);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult();
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.AreEqual(5.0, result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.AreEqual(20.0, result.ScrumTeam.EstimationResult[1].Estimation!.Value);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsInfinity()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: PlanningPokerData.InfinityEstimation, memberEstimation: null);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.AreEqual(PlanningPokerData.InfinityEstimation, result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.IsNull(result.ScrumTeam.EstimationResult[1].Estimation!.Value);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationCanceled()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: 0, memberEstimation: double.NaN);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, state: TeamState.EstimationCanceled, estimationResult: estimationResult);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.AreEqual(0.0, result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.IsNull(result.ScrumTeam.EstimationResult[1].Estimation);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationInProgress()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationParticipants = PlanningPokerData.GetEstimationParticipants();
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, state: TeamState.EstimationInProgress, estimationParticipants: estimationParticipants);
        var teamResult = PlanningPokerData.GetTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, teamResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(teamResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
    }

    [TestMethod]
    public async Task JoinTeam_TeamDoesNotExist_PlanningPokerException()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var errorMessage = @"PlanningPokerException:{""Error"":""MemberAlreadyExists"",""Message"":""Member or observer named \u0027member\u0027 already exists in the team."",""Argument"":""member""}";
        var returnMessage = new CompletionMessage(invocationId, errorMessage, null, false);
        await fixture.ReceiveMessage(returnMessage);

        var exception = await Assert.ThrowsExactlyAsync<PlanningPokerException>(() => resultTask);

        Assert.AreEqual(ErrorCodes.MemberAlreadyExists, exception.Error);
        Assert.AreEqual("member", exception.Argument);
        Assert.AreEqual("Member or observer named 'member' already exists in the team.", exception.Message);
    }

    [TestMethod]
    public async Task JoinTeam_ReturnsEmptyErrorMessage_PlanningPokerException()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var returnMessage = new CompletionMessage(invocationId, "An unexpected error occured. HubException: ", null, false);
        await fixture.ReceiveMessage(returnMessage);

        var exception = await Assert.ThrowsExactlyAsync<PlanningPokerException>(() => resultTask);

        Assert.AreEqual(string.Empty, exception.Message);
        Assert.IsNull(exception.Error);
        Assert.IsNull(exception.Argument);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamNameAndMemberName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("ReconnectTeam", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeam()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam);
        var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(reconnectResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNull(reconnectResult.SelectedEstimation);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamAndLastMessageId()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: 1, memberEstimation: 1);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam, lastMessageId: 123);
        var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(reconnectResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNull(reconnectResult.SelectedEstimation);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.AreEqual(1.0, result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.AreEqual(1.0, result.ScrumTeam.EstimationResult[1].Estimation!.Value);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: null, memberEstimation: PlanningPokerData.UnknownEstimation);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam, lastMessageId: 123, selectedEstimation: PlanningPokerData.UnknownEstimation);
        var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(reconnectResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.AreEqual(PlanningPokerData.UnknownEstimation, result.SelectedEstimation.Value!);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.IsNull(result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.AreEqual(PlanningPokerData.UnknownEstimation, result.ScrumTeam.EstimationResult[1].Estimation!.Value);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsNull()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: 8, memberEstimation: double.NaN);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam, lastMessageId: 2157483849, selectedEstimation: 8);
        var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(reconnectResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.AreEqual(8.0, result.SelectedEstimation.Value);
        Assert.IsNotNull(result.ScrumTeam.EstimationResult);
        Assert.AreEqual(8.0, result.ScrumTeam.EstimationResult[0].Estimation!.Value);
        Assert.IsNull(result.ScrumTeam.EstimationResult[1].Estimation);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationInProgress()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var estimationParticipants = PlanningPokerData.GetEstimationParticipants(scrumMaster: false, member: true);
        var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, state: TeamState.EstimationInProgress, estimationParticipants: estimationParticipants);
        var reconnectResult = PlanningPokerData.GetReconnectTeamResult(scrumTeam, lastMessageId: 1, selectedEstimation: null);
        var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(reconnectResult, result);
        Assert.IsNotNull(result.ScrumTeam);
        AssertAvailableEstimations(result.ScrumTeam);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.IsNull(result.SelectedEstimation.Value);
    }

    [TestMethod]
    public async Task DisconnectTeam_TeamNameAndMemberName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("DisconnectTeam", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task StartEstimation_TeamName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.StartEstimation(PlanningPokerData.TeamName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("StartEstimation", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task CancelEstimation_TeamName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CancelEstimation(PlanningPokerData.TeamName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("CancelEstimation", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task CloseEstimation_TeamName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CloseEstimation(PlanningPokerData.TeamName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("CloseEstimation", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    [DataRow(PlanningPokerData.MemberName, 3.0, 3.0)]
    [DataRow(PlanningPokerData.ScrumMasterName, 0.0, 0.0)]
    [DataRow(PlanningPokerData.MemberName, 100.0, 100.0)]
    [DataRow(PlanningPokerData.MemberName, PlanningPokerData.InfinityEstimation, PlanningPokerData.InfinityEstimation)]
    [DataRow(PlanningPokerData.MemberName, PlanningPokerData.UnknownEstimation, PlanningPokerData.UnknownEstimation)]
    [DataRow(PlanningPokerData.MemberName, null, null)]
    public async Task SubmitEstimation_EstimationValue_InvocationMessageIsSent(string memberName, double? estimation, double? expectedSentValue)
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.SubmitEstimation(PlanningPokerData.TeamName, memberName, estimation, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("SubmitEstimation", sentInvocationMessage.Target);
        var expectedArguments = new object?[] { PlanningPokerData.TeamName, memberName, expectedSentValue };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    [DataRow(Deck.Standard)]
    [DataRow(Deck.Rating)]
    [DataRow(Deck.Tshirt)]
    public async Task ChangeDeck_SelectedDeck_InvocationMessageIsSent(Deck deck)
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.ChangeDeck(PlanningPokerData.TeamName, deck, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("ChangeDeck", sentInvocationMessage.Target);
        var expectedArguments = new object?[] { PlanningPokerData.TeamName, deck };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task StartTimer_TeamNameAndDuration_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.StartTimer(PlanningPokerData.TeamName, PlanningPokerData.MemberName, TimeSpan.FromSeconds(264), fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("StartTimer", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, TimeSpan.FromSeconds(264) };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task CancelTimer_TeamName_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.CancelTimer(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("CancelTimer", sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task GetCurrentTime_InvocationMessageIsSent()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.GetCurrentTime(fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
        Assert.AreEqual("GetCurrentTime", sentInvocationMessage.Target);
        Assert.IsEmpty(sentInvocationMessage.Arguments);

        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2022, 5, 5, 0, 8, 30, DateTimeKind.Utc)
        };
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, timeResult, true);
        await fixture.ReceiveMessage(returnMessage);

        await resultTask;
    }

    [TestMethod]
    public async Task GetCurrentTime_ReturnsCurrentTime()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.GetCurrentTime(fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        var invocationId = GetInvocationId(sentMessage);

        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2022, 5, 5, 0, 8, 30, DateTimeKind.Utc)
        };
        var returnMessage = new CompletionMessage(invocationId, null, timeResult, true);
        await fixture.ReceiveMessage(returnMessage);

        var result = await resultTask;

        Assert.AreEqual(new DateTime(2022, 5, 5, 0, 8, 30, DateTimeKind.Utc), result.CurrentUtcTime);
    }

    private static string GetInvocationId([NotNull] HubMessage? message)
    {
        var invocationMessage = AssertIsInvocationMessage(message);
        Assert.IsNotNull(invocationMessage.InvocationId);
        return invocationMessage.InvocationId;
    }

    private static InvocationMessage AssertIsInvocationMessage([NotNull] HubMessage? message)
    {
        Assert.IsNotNull(message);
        var invocationMessage = Assert.IsInstanceOfType<InvocationMessage>(message);
        return invocationMessage;
    }

    private static void AssertAvailableEstimations(ScrumTeam scrumTeam)
    {
        Assert.HasCount(13, scrumTeam.AvailableEstimations);
        Assert.AreEqual(0.0, scrumTeam.AvailableEstimations[0].Value);
        Assert.AreEqual(0.5, scrumTeam.AvailableEstimations[1].Value);
        Assert.AreEqual(1.0, scrumTeam.AvailableEstimations[2].Value);
        Assert.AreEqual(2.0, scrumTeam.AvailableEstimations[3].Value);
        Assert.AreEqual(3.0, scrumTeam.AvailableEstimations[4].Value);
        Assert.AreEqual(5.0, scrumTeam.AvailableEstimations[5].Value);
        Assert.AreEqual(8.0, scrumTeam.AvailableEstimations[6].Value);
        Assert.AreEqual(13.0, scrumTeam.AvailableEstimations[7].Value);
        Assert.AreEqual(20.0, scrumTeam.AvailableEstimations[8].Value);
        Assert.AreEqual(40.0, scrumTeam.AvailableEstimations[9].Value);
        Assert.AreEqual(100.0, scrumTeam.AvailableEstimations[10].Value);
        Assert.AreEqual(PlanningPokerData.InfinityEstimation, scrumTeam.AvailableEstimations[11].Value);
        Assert.AreEqual(PlanningPokerData.UnknownEstimation, scrumTeam.AvailableEstimations[12].Value);
    }
}
