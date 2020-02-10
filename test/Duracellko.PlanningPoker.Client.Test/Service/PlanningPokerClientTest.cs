using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Mock object does not need to be disposed.")]
    public class PlanningPokerClientTest
    {
        internal const string BaseUrl = "http://planningpoker.duracellko.net/";
        internal const string JsonType = "application/json";
        internal const string TextType = "text/plain";

        [TestMethod]
        public async Task CreateTeam_TeamAndScrumMasterName_RequestsCreateTeamUrl()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CreateTeam?teamName={PlanningPokerClientData.TeamName}&scrumMasterName={PlanningPokerClientData.ScrumMasterName}")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson());
            var target = CreatePlanningPokerClient(httpMock);

            await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task CreateTeam_TeamAndScrumMasterName_ReturnsScrumTeam()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson());
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(1, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimations(result);

            Assert.AreEqual(0, result.EstimationResult.Count);
            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameExists_PlanningPokerException()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
                .Respond(HttpStatusCode.BadRequest, TextType, "Team 'Test team' already exists.");
            var target = CreatePlanningPokerClient(httpMock);

            var exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None));

            Assert.AreEqual("Team 'Test team' already exists.", exception.Message);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_RequestsJoinTeamUrl()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&asObserver=False")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true));
            var target = CreatePlanningPokerClient(httpMock);

            await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndObserverName_RequestsJoinTeamUrl()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ObserverName}&asObserver=True")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(observer: true));
            var target = CreatePlanningPokerClient(httpMock);

            await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeam()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimations(result);

            Assert.AreEqual(0, result.EstimationResult.Count);
            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndObserverName_ReturnsScrumTeam()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(observer: true));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(1, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimations(result);

            Assert.AreEqual(0, result.EstimationResult.Count);
            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
        {
            var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson();
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimationFinished, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimations(result);

            Assert.AreEqual(2, result.EstimationResult.Count);
            var estimationResult = result.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(5.0, estimationResult.Estimation.Value);

            estimationResult = result.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.AreEqual(20.0, estimationResult.Estimation.Value);

            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsInfinity()
        {
            var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "-1111100", memberEstimation: "null");
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimationFinished, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimations(result);

            Assert.AreEqual(2, result.EstimationResult.Count);
            var estimationResult = result.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimation.Value.Value));

            estimationResult = result.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimation.Value);

            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationCanceled()
        {
            var estimationResultJson = PlanningPokerClientData.GetEstimationResultJson(scrumMasterEstimation: "0", memberEstimation: null);
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, state: 3, estimationResult: estimationResultJson));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimationCanceled, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimations(result);

            Assert.AreEqual(2, result.EstimationResult.Count);
            var estimationResult = result.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(0.0, estimationResult.Estimation.Value);

            estimationResult = result.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimation);

            Assert.AreEqual(0, result.EstimationParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationInProgress()
        {
            var estimationParticipantsJson = PlanningPokerClientData.GetEstimationParticipantsJson();
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, state: 1, estimationParticipants: estimationParticipantsJson));
            var target = CreatePlanningPokerClient(httpMock);

            var result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimationInProgress, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            var member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimations(result);

            Assert.AreEqual(0, result.EstimationResult.Count);

            Assert.AreEqual(2, result.EstimationParticipants.Count);
            var estimationParticipant = result.EstimationParticipants[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationParticipant.MemberName);
            Assert.IsTrue(estimationParticipant.Estimated);

            estimationParticipant = result.EstimationParticipants[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationParticipant.MemberName);
            Assert.IsFalse(estimationParticipant.Estimated);
        }

        [TestMethod]
        public async Task JoinTeam_TeamDoesNotExist_PlanningPokerException()
        {
            var httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/JoinTeam")
                .Respond(HttpStatusCode.BadRequest, TextType, "Team 'Test team' does not exist.");
            var target = CreatePlanningPokerClient(httpMock);

            var exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None));

            Assert.AreEqual("Team 'Test team' does not exist.", exception.Message);
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
            Assert.AreEqual(0, result.LastMessageId);
            Assert.IsNull(result.SelectedEstimation);

            var scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.Initial, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
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

            Assert.AreEqual(0, scrumTeam.EstimationResult.Count);
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
            Assert.AreEqual(123, result.LastMessageId);
            Assert.IsNull(result.SelectedEstimation);

            var scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
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

            Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
            var estimationResult = scrumTeam.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(1.0, estimationResult.Estimation.Value);

            estimationResult = scrumTeam.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.AreEqual(1.0, estimationResult.Estimation.Value);

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
            Assert.AreEqual(123, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.IsNotNull(result.SelectedEstimation.Value);
            Assert.IsTrue(double.IsPositiveInfinity(result.SelectedEstimation.Value.Value));

            var scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
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

            Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
            var estimationResult = scrumTeam.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimation.Value);

            estimationResult = scrumTeam.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimation.Value.Value));

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
            Assert.AreEqual(2157483849, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.AreEqual(8.0, result.SelectedEstimation.Value);

            var scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimationFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
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

            Assert.AreEqual(2, scrumTeam.EstimationResult.Count);
            var estimationResult = scrumTeam.EstimationResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(8.0, estimationResult.Estimation.Value);

            estimationResult = scrumTeam.EstimationResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimation);

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
            Assert.AreEqual(1, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.IsNull(result.SelectedEstimation.Value);

            var scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimationInProgress, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
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

            Assert.AreEqual(0, scrumTeam.EstimationResult.Count);

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
            Assert.IsTrue(double.IsPositiveInfinity(scrumTeam.AvailableEstimations[11].Value.Value));
            Assert.IsNull(scrumTeam.AvailableEstimations[12].Value);
        }
    }
}
