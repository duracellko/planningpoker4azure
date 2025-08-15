using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Duracellko.PlanningPoker.Client.Test.Service;

[TestClass]
[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Mock object does not need to be disposed.")]
public class PlanningPokerClientTest
{
    internal const string BaseUrl = "http://planningpoker.duracellko.net/";
    internal const string JsonType = "application/json";
    internal const string TextType = "text/plain";

    [TestMethod]
    [DataRow(Deck.Standard, nameof(Deck.Standard))]
    [DataRow(Deck.Fibonacci, nameof(Deck.Fibonacci))]
    [DataRow(Deck.RockPaperScissorsLizardSpock, nameof(Deck.RockPaperScissorsLizardSpock))]
    public async Task CreateTeam_TeamAndScrumMasterName_RequestsCreateTeamUrl(Deck deck, string deckValue)
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CreateTeam?teamName={PlanningPokerClientData.TeamName}&scrumMasterName={PlanningPokerClientData.ScrumMasterName}&deck={deckValue}")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, deck, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task CreateTeam_TeamAndScrumMasterName_ReturnsScrumTeam()
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson();
        httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, Deck.Standard, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.Initial, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(1, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        Assert.AreEqual(0, resultTeam.Observers.Count);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(0, resultTeam.EstimationResult.Count);
        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task CreateTeam_TeamNameExists_PlanningPokerException()
    {
        var httpMock = new MockHttpMessageHandler();
        var response = @"PlanningPokerException:{""Error"":""ScrumTeamAlreadyExists"",""Message"":""Cannot create Scrum Team \u0027test team\u0027. Team with that name already exists."",""Argument"":""test team""}";
        httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
            .Respond(HttpStatusCode.BadRequest, TextType, response);
        var target = CreatePlanningPokerClient(httpMock);

        var exception = await Assert.ThrowsExactlyAsync<PlanningPokerException>(
            () => target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, Deck.Standard, CancellationToken.None));

        Assert.AreEqual(ErrorCodes.ScrumTeamAlreadyExists, exception.Error);
        Assert.AreEqual("test team", exception.Argument);
        Assert.AreEqual("Cannot create Scrum Team 'test team'. Team with that name already exists.", exception.Message);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_RequestsJoinTeamUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&asObserver=False")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndObserverName_RequestsJoinTeamUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(observer: true);
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ObserverName}&asObserver=True")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeam()
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.Initial, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(2, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = resultTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, resultTeam.Observers.Count);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(0, resultTeam.EstimationResult.Count);
        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndObserverName_ReturnsScrumTeam()
    {
        var httpMock = new MockHttpMessageHandler();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(observer: true);
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.Initial, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(1, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        Assert.AreEqual(1, resultTeam.Observers.Count);
        member = resultTeam.Observers[0];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(0, resultTeam.EstimationResult.Count);
        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.EstimationFinished, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(2, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = resultTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(1, resultTeam.Observers.Count);
        member = resultTeam.Observers[0];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(2, resultTeam.EstimationResult.Count);
        var estimationResult = resultTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(5.0, estimationResult.Estimation!.Value);

        estimationResult = resultTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.AreEqual(20.0, estimationResult.Estimation!.Value);

        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsInfinity()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "-1111100", memberEstimation: "null");
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.EstimationFinished, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(2, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = resultTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(1, resultTeam.Observers.Count);
        member = resultTeam.Observers[0];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(2, resultTeam.EstimationResult.Count);
        var estimationResult = resultTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimation!.Value!.Value));

        estimationResult = resultTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation!.Value);

        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationCanceled()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "0", memberEstimation: null);
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 3, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.EstimationCanceled, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(2, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = resultTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, resultTeam.Observers.Count);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(2, resultTeam.EstimationResult.Count);
        var estimationResult = resultTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(0.0, estimationResult.Estimation!.Value);

        estimationResult = resultTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation);

        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(0, resultTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationInProgress()
    {
        var estimationParticipantsJson = PlanningPokerClientData.GetEstimationParticipantsJson();
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 1, estimationParticipants: estimationParticipantsJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
            .Respond(JsonType, PlanningPokerClientData.GetTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(PlanningPokerClientData.SessionId, result.SessionId);

        var resultTeam = result.ScrumTeam;
        Assert.IsNotNull(resultTeam);
        Assert.AreEqual(TeamState.EstimationInProgress, resultTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, resultTeam.Name);
        Assert.IsNotNull(resultTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, resultTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, resultTeam.ScrumMaster.Type);

        Assert.AreEqual(2, resultTeam.Members.Count);
        var member = resultTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = resultTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, resultTeam.Observers.Count);

        AssertAvailableEstimations(resultTeam);

        Assert.IsNotNull(resultTeam.EstimationResult);
        Assert.AreEqual(0, resultTeam.EstimationResult.Count);

        Assert.IsNotNull(resultTeam.EstimationParticipants);
        Assert.AreEqual(2, resultTeam.EstimationParticipants.Count);
        var estimationParticipant = resultTeam.EstimationParticipants[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationParticipant.MemberName);
        Assert.IsTrue(estimationParticipant.Estimated);

        estimationParticipant = resultTeam.EstimationParticipants[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationParticipant.MemberName);
        Assert.IsFalse(estimationParticipant.Estimated);
    }

    [TestMethod]
    public async Task JoinTeam_TeamDoesNotExist_PlanningPokerException()
    {
        var httpMock = new MockHttpMessageHandler();
        var response = @"PlanningPokerException:{""Error"":""ScrumTeamNotExist"",""Message"":""Scrum Team \u0027test team\u0027 does not exist."",""Argument"":""test team""}";
        httpMock.When(BaseUrl + "api/PlanningPokerService/JoinTeam")
            .Respond(HttpStatusCode.BadRequest, TextType, response);
        var target = CreatePlanningPokerClient(httpMock);

        var exception = await Assert.ThrowsExactlyAsync<PlanningPokerException>(() => target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None));

        Assert.AreEqual(ErrorCodes.ScrumTeamNotExist, exception.Error);
        Assert.AreEqual("test team", exception.Argument);
        Assert.AreEqual("Scrum Team 'test team' does not exist.", exception.Message);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_RequestsJoinTeamUrl()
    {
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/ReconnectTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeam()
    {
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual(PlanningPokerClientData.ReconnectSessionId, result.SessionId);
        Assert.AreEqual(0, result.LastMessageId);
        Assert.IsNull(result.SelectedEstimation);

        var scrumTeam = result.ScrumTeam;
        Assert.AreEqual(TeamState.Initial, scrumTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
        Assert.IsNotNull(scrumTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

        Assert.AreEqual(2, scrumTeam.Members.Count);
        var member = scrumTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = scrumTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, scrumTeam.Observers.Count);

        AssertAvailableEstimations(scrumTeam);

        Assert.IsNotNull(scrumTeam.EstimationResult);
        Assert.AreEqual(0, scrumTeam.EstimationResult.Count);
        Assert.IsNotNull(scrumTeam.EstimationParticipants);
        Assert.AreEqual(0, scrumTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamAndLastMessageId()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "1", memberEstimation: "1");
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "123"));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual(PlanningPokerClientData.ReconnectSessionId, result.SessionId);
        Assert.AreEqual(123, result.LastMessageId);
        Assert.IsNull(result.SelectedEstimation);

        var scrumTeam = result.ScrumTeam;
        Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
        Assert.IsNotNull(scrumTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

        Assert.AreEqual(2, scrumTeam.Members.Count);
        var member = scrumTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = scrumTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(1, scrumTeam.Observers.Count);
        member = scrumTeam.Observers[0];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

        AssertAvailableEstimations(scrumTeam);

        Assert.IsNotNull(scrumTeam.EstimationResult);
        Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
        var estimationResult = scrumTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(1.0, estimationResult.Estimation!.Value);

        estimationResult = scrumTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.AreEqual(1.0, estimationResult.Estimation!.Value);

        Assert.IsNotNull(scrumTeam.EstimationParticipants);
        Assert.AreEqual(0, scrumTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "null", memberEstimation: "-1111100");
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "123", selectedEstimation: "-1111100"));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual(PlanningPokerClientData.ReconnectSessionId, result.SessionId);
        Assert.AreEqual(123, result.LastMessageId);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.IsNotNull(result.SelectedEstimation.Value);
        Assert.IsTrue(double.IsPositiveInfinity(result.SelectedEstimation.Value.Value));

        var scrumTeam = result.ScrumTeam;
        Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
        Assert.IsNotNull(scrumTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

        Assert.AreEqual(2, scrumTeam.Members.Count);
        var member = scrumTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = scrumTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(1, scrumTeam.Observers.Count);
        member = scrumTeam.Observers[0];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

        AssertAvailableEstimations(scrumTeam);

        Assert.IsNotNull(scrumTeam.EstimationResult);
        Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
        var estimationResult = scrumTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation!.Value);

        estimationResult = scrumTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimation!.Value!.Value));

        Assert.IsNotNull(scrumTeam.EstimationParticipants);
        Assert.AreEqual(0, scrumTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsNull()
    {
        var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "8", memberEstimation: null);
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 2, estimationResult: estimationResultJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "2157483849", selectedEstimation: "8"));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual(PlanningPokerClientData.ReconnectSessionId, result.SessionId);
        Assert.AreEqual(2157483849, result.LastMessageId);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.AreEqual(8.0, result.SelectedEstimation.Value);

        var scrumTeam = result.ScrumTeam;
        Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
        Assert.IsNotNull(scrumTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

        Assert.AreEqual(2, scrumTeam.Members.Count);
        var member = scrumTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = scrumTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, scrumTeam.Observers.Count);

        AssertAvailableEstimations(scrumTeam);

        Assert.IsNotNull(scrumTeam.EstimationResult);
        Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
        var estimationResult = scrumTeam.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(8.0, estimationResult.Estimation!.Value);

        estimationResult = scrumTeam.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation);

        Assert.IsNotNull(scrumTeam.EstimationParticipants);
        Assert.AreEqual(0, scrumTeam.EstimationParticipants.Count);
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationInProgress()
    {
        var estimationParticipantsJson = PlanningPokerClientData.GetEstimationParticipantsJson(scrumMaster: false, member: true);
        var scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 1, estimationParticipants: estimationParticipantsJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
            .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "1", selectedEstimation: "null"));
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        Assert.IsNotNull(result.ScrumTeam);
        Assert.AreEqual(PlanningPokerClientData.ReconnectSessionId, result.SessionId);
        Assert.AreEqual(1, result.LastMessageId);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.IsNull(result.SelectedEstimation.Value);

        var scrumTeam = result.ScrumTeam;
        Assert.AreEqual(TeamState.EstimationInProgress, scrumTeam.State);
        Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
        Assert.IsNotNull(scrumTeam.ScrumMaster);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

        Assert.AreEqual(2, scrumTeam.Members.Count);
        var member = scrumTeam.Members[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

        member = scrumTeam.Members[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

        Assert.AreEqual(0, scrumTeam.Observers.Count);

        AssertAvailableEstimations(scrumTeam);

        Assert.IsNotNull(scrumTeam.EstimationResult);
        Assert.AreEqual(0, scrumTeam.EstimationResult.Count);

        Assert.IsNotNull(scrumTeam.EstimationParticipants);
        Assert.AreEqual(2, scrumTeam.EstimationParticipants.Count);
        var estimationParticipant = scrumTeam.EstimationParticipants[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationParticipant.MemberName);
        Assert.IsFalse(estimationParticipant.Estimated);

        estimationParticipant = scrumTeam.EstimationParticipants[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationParticipant.MemberName);
        Assert.IsTrue(estimationParticipant.Estimated);
    }

    [TestMethod]
    public async Task DisconnectTeam_TeamAndMemberName_RequestsDisconnectTeamUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/DisconnectTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.DisconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task StartEstimation_TeamName_RequestsStartEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/StartEstimation?teamName={PlanningPokerClientData.TeamName}")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.StartEstimation(PlanningPokerClientData.TeamName, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task CancelEstimation_TeamName_RequestsCancelEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CancelEstimation?teamName={PlanningPokerClientData.TeamName}")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.CancelEstimation(PlanningPokerClientData.TeamName, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task SubmitEstimation_3_RequestsSubmitEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimation?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimation=3")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.SubmitEstimation(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 3, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task SubmitEstimation_0_RequestsSubmitEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimation?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ScrumMasterName}&estimation=0")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.SubmitEstimation(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, 0, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task SubmitEstimation_100_RequestsSubmitEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimation?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimation=100")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.SubmitEstimation(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 100, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task SubmitEstimation_PositiveInfinity_RequestsSubmitEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimation?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimation=-1111100")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.SubmitEstimation(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, double.PositiveInfinity, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task SubmitEstimation_Null_RequestsSubmitEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimation?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimation=-1111111")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.SubmitEstimation(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, null, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    [DataRow(Deck.Standard, nameof(Deck.Standard))]
    [DataRow(Deck.Rating, nameof(Deck.Rating))]
    [DataRow(Deck.Tshirt, nameof(Deck.Tshirt))]
    public async Task ChangeDeck_SelectedDeck_InvocationMessageIsSent(Deck deck, string deckValue)
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/ChangeDeck?teamName={PlanningPokerClientData.TeamName}&deck={deckValue}")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.ChangeDeck(PlanningPokerClientData.TeamName, deck, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task StartTimer_TeamNameAndDuration_RequestsStartEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/StartTimer?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&duration=264")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.StartTimer(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, TimeSpan.FromSeconds(264), CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task CancelTimer_TeamName_RequestsCancelEstimationUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CancelTimer?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}")
            .Respond(TextType, string.Empty);
        var target = CreatePlanningPokerClient(httpMock);

        await target.CancelTimer(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task GetCurrentTime_RequestsGetCurrentTimeUrl()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/GetCurrentTime")
            .Respond(JsonType, PlanningPokerClientData.CurrentTimeJson);
        var target = CreatePlanningPokerClient(httpMock);

        await target.GetCurrentTime(CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task GetCurrentTime_ReturnsCurrentTime()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(BaseUrl + $"api/PlanningPokerService/GetCurrentTime")
            .Respond(JsonType, PlanningPokerClientData.CurrentTimeJson);
        var target = CreatePlanningPokerClient(httpMock);

        var result = await target.GetCurrentTime(CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(new DateTime(2022, 5, 6, 16, 43, 21, DateTimeKind.Utc), result.CurrentUtcTime);
    }

    internal static PlanningPokerClient CreatePlanningPokerClient(MockHttpMessageHandler messageHandler)
    {
        var httpClient = messageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        return new PlanningPokerClient(httpClient);
    }

    private static void AssertAvailableEstimations(ScrumTeam scrumTeam)
    {
        Assert.AreEqual(13, scrumTeam.AvailableEstimations.Count);
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
        Assert.AreEqual(100.0, scrumTeam.AvailableEstimations[10].Value);
        var estimationValue = scrumTeam.AvailableEstimations[11].Value;
        Assert.IsNotNull(estimationValue);
        Assert.IsTrue(double.IsPositiveInfinity(estimationValue.Value));
        Assert.IsNull(scrumTeam.AvailableEstimations[12].Value);
    }
}
