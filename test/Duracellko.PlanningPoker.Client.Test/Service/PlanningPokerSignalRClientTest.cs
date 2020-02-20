using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    public class PlanningPokerSignalRClientTest
    {
        [TestMethod]
        public async Task CreateTeam_TeamNameAndScrumMaster_InvocationMessageIsSent()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
            Assert.AreEqual("CreateTeam", sentInvocationMessage.Target);
            var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName };
            CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            await resultTask;
        }

        [TestMethod]
        public async Task CreateTeam_TeamAndScrumMasterName_ReturnsScrumTeam()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameExists_PlanningPokerException()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var returnMessage = new CompletionMessage(invocationId, "Team 'Test team' already exists.", null, false);
            await fixture.ReceiveMessage(returnMessage);

            var exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => resultTask);

            Assert.AreEqual("Team 'Test team' already exists.", exception.Message);
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
            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, scrumTeam, true);
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
            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, scrumTeam, true);
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
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndObserverName_ReturnsScrumTeam()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, false, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var scrumTeam = PlanningPokerData.GetScrumTeam(observer: true);
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
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
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
            Assert.AreEqual(5.0, result.EstimationResult[0].Estimation.Value);
            Assert.AreEqual(20.0, result.EstimationResult[1].Estimation.Value);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinishedAndEstimationIsInfinity()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: Estimation.PositiveInfinity, memberEstimation: null);
            var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
            Assert.IsTrue(double.IsPositiveInfinity(result.EstimationResult[0].Estimation.Value.Value));
            Assert.IsNull(result.EstimationResult[1].Estimation.Value);
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
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
            Assert.AreEqual(0.0, result.EstimationResult[0].Estimation.Value);
            Assert.IsNull(result.EstimationResult[1].Estimation);
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
            var returnMessage = new CompletionMessage(invocationId, null, scrumTeam, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(scrumTeam, result);
            AssertAvailableEstimations(result);
        }

        [TestMethod]
        public async Task JoinTeam_TeamDoesNotExist_PlanningPokerException()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var returnMessage = new CompletionMessage(invocationId, "Team 'Test team' does not exist.", null, false);
            await fixture.ReceiveMessage(returnMessage);

            var exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => resultTask);

            Assert.AreEqual("Team 'Test team' does not exist.", exception.Message);
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
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam);
            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, reconnectResult, true);
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
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam);
            var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(reconnectResult, result);
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
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam, lastMessageId: 123);
            var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(reconnectResult, result);
            AssertAvailableEstimations(result.ScrumTeam);
            Assert.IsNull(reconnectResult.SelectedEstimation);
            Assert.AreEqual(1.0, result.ScrumTeam.EstimationResult[0].Estimation.Value);
            Assert.AreEqual(1.0, result.ScrumTeam.EstimationResult[1].Estimation.Value);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimationFinished()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var invocationId = GetInvocationId(sentMessage);

            var estimationResult = PlanningPokerData.GetEstimationResult(scrumMasterEstimation: null, memberEstimation: Estimation.PositiveInfinity);
            var scrumTeam = PlanningPokerData.GetScrumTeam(member: true, observer: true, state: TeamState.EstimationFinished, estimationResult: estimationResult);
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam, lastMessageId: 123, selectedEstimation: Estimation.PositiveInfinity);
            var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(reconnectResult, result);
            AssertAvailableEstimations(result.ScrumTeam);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.IsTrue(double.IsPositiveInfinity(result.SelectedEstimation.Value.Value));
            Assert.IsNull(result.ScrumTeam.EstimationResult[0].Estimation.Value);
            Assert.IsTrue(double.IsPositiveInfinity(result.ScrumTeam.EstimationResult[1].Estimation.Value.Value));
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
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam, lastMessageId: 2157483849, selectedEstimation: 8);
            var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(reconnectResult, result);
            AssertAvailableEstimations(result.ScrumTeam);
            Assert.IsNotNull(result.SelectedEstimation);
            Assert.AreEqual(8.0, result.SelectedEstimation.Value);
            Assert.AreEqual(8.0, result.ScrumTeam.EstimationResult[0].Estimation.Value);
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
            var reconnectResult = PlanningPokerData.GetReconnectTeamResultJson(scrumTeam, lastMessageId: 1, selectedEstimation: null);
            var returnMessage = new CompletionMessage(invocationId, null, reconnectResult, true);
            await fixture.ReceiveMessage(returnMessage);

            var result = await resultTask;

            Assert.AreEqual(reconnectResult, result);
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

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
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

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
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

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
            await fixture.ReceiveMessage(returnMessage);

            await resultTask;
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.MemberName, 3.0, 3.0)]
        [DataRow(PlanningPokerData.ScrumMasterName, 0.0, 0.0)]
        [DataRow(PlanningPokerData.MemberName, 100.0, 100.0)]
        [DataRow(PlanningPokerData.MemberName, double.PositiveInfinity, Estimation.PositiveInfinity)]
        [DataRow(PlanningPokerData.MemberName, null, -1111111.0)]
        public async Task SubmitEstimation_EstimationValue_InvocationMessageIsSent(string memberName, double? estimation, double expectedSentValue)
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.SubmitEstimation(PlanningPokerData.TeamName, memberName, estimation, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            var sentInvocationMessage = AssertIsInvocationMessage(sentMessage);
            Assert.AreEqual("SubmitEstimation", sentInvocationMessage.Target);
            var expectedArguments = new object[] { PlanningPokerData.TeamName, memberName, expectedSentValue };
            CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
            await fixture.ReceiveMessage(returnMessage);

            await resultTask;
        }

        private static string GetInvocationId(HubMessage message)
        {
            var invocationMessage = AssertIsInvocationMessage(message);
            return invocationMessage.InvocationId;
        }

        private static InvocationMessage AssertIsInvocationMessage(HubMessage message)
        {
            Assert.IsNotNull(message);
            Assert.IsInstanceOfType(message, typeof(InvocationMessage));
            return (InvocationMessage)message;
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
