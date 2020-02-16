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
