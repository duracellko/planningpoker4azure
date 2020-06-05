using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    public class PlanningPokerSignalRClientTestMessages
    {
        private const string RequestName = "GetMessages";
        private const string ResponseName = "Notify";

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_InvocationMessageIsSent()
        {
            var sessionId = Guid.NewGuid();
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, sessionId, 0, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            Assert.IsNotNull(sentMessage);
            Assert.IsInstanceOfType(sentMessage, typeof(InvocationMessage));
            var sentInvocationMessage = (InvocationMessage)sentMessage;
            Assert.AreEqual(RequestName, sentInvocationMessage.Target);
            var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, sessionId, 0L };
            CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
            await fixture.ReceiveMessage(returnMessage);

            var notifyMessage = new InvocationMessage(ResponseName, new object[] { new List<Message>() });
            await fixture.ReceiveMessage(notifyMessage);

            await resultTask;

            // Ensure asynchronous Dispose. Otherwise, Dispose is executed in OnNotify handler and it causes deadlock.
            await Task.Yield();
        }

        [TestMethod]
        public async Task GetMessages_LastMessageId_InvocationMessageIsSent()
        {
            var sessionId = Guid.NewGuid();
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, sessionId, 2157483849, fixture.CancellationToken);

            var sentMessage = await fixture.GetSentMessage();
            Assert.IsNotNull(sentMessage);
            Assert.IsInstanceOfType(sentMessage, typeof(InvocationMessage));
            var sentInvocationMessage = (InvocationMessage)sentMessage;
            Assert.AreEqual(RequestName, sentInvocationMessage.Target);
            var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, sessionId, 2157483849L };
            CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

            var notifyMessage = new InvocationMessage(ResponseName, new object[] { new List<Message>() });
            await fixture.ReceiveMessage(notifyMessage);

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
            await fixture.ReceiveMessage(returnMessage);

            await resultTask;

            // Ensure asynchronous Dispose. Otherwise, Dispose is executed in OnNotify handler and it causes deadlock.
            await Task.Yield();
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEmptyMessage()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var message = new Message();
            await ProvideMessages(fixture, message);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message, result[0]);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsMemberJoinedMessage()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var message = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberJoined,
                Member = new TeamMember { Name = PlanningPokerData.MemberName, Type = PlanningPokerData.MemberType }
            };
            await ProvideMessages(fixture, message);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message, result[0]);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsMemberDisconnectedMessage()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var message = new MemberMessage
            {
                Id = 2,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember { Name = PlanningPokerData.ObserverName, Type = PlanningPokerData.ObserverType }
            };
            await ProvideMessages(fixture, message);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message, result[0]);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEstimationStartedMessage()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var message = new Message
            {
                Id = 2157483849,
                Type = MessageType.EstimationStarted
            };
            await ProvideMessages(fixture, message);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message, result[0]);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEstimationEndedMessage()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var message = new EstimationResultMessage
            {
                Id = 8,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
                {
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = PlanningPokerData.ScrumMasterName, Type = PlanningPokerData.ScrumMasterType },
                        Estimation = new Estimation { Value = 2 }
                    },
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = PlanningPokerData.MemberName, Type = PlanningPokerData.MemberType },
                        Estimation = new Estimation()
                    },
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = "Me", Type = PlanningPokerData.MemberType }
                    },
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = PlanningPokerData.ObserverName, Type = PlanningPokerData.MemberType },
                        Estimation = new Estimation { Value = Estimation.PositiveInfinity }
                    }
                }
            };
            await ProvideMessages(fixture, message);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message, result[0]);
            var estimationResult = ((EstimationResultMessage)result[0]).EstimationResult;
            Assert.AreEqual(2.0, estimationResult[0].Estimation.Value);
            Assert.IsNull(estimationResult[1].Estimation.Value);
            Assert.IsNull(estimationResult[2].Estimation);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult[3].Estimation.Value.Value));
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_Returns3Messages()
        {
            await using var fixture = new PlanningPokerSignalRClientFixture();

            var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

            var estimationStartedMessage = new Message
            {
                Id = 8,
                Type = MessageType.EstimationStarted
            };
            var memberEstimatedMessage = new MemberMessage
            {
                Id = 9,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember { Name = PlanningPokerData.ScrumMasterName, Type = PlanningPokerData.ScrumMasterType }
            };
            var estimationEndedMessage = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.MemberEstimated,
                EstimationResult = new List<EstimationResultItem>
                {
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = PlanningPokerData.ScrumMasterName, Type = PlanningPokerData.ScrumMasterType },
                        Estimation = new Estimation { Value = 5 }
                    },
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = PlanningPokerData.MemberName, Type = PlanningPokerData.MemberType },
                        Estimation = new Estimation { Value = 40 }
                    }
                }
            };
            await ProvideMessages(fixture, estimationStartedMessage, memberEstimatedMessage, estimationEndedMessage);

            var result = await resultTask;
            await Task.Yield();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(estimationStartedMessage, result[0]);
            Assert.AreEqual(memberEstimatedMessage, result[1]);
            Assert.AreEqual(estimationEndedMessage, result[2]);

            var estimationResult = ((EstimationResultMessage)result[2]).EstimationResult;
            Assert.AreEqual(5.0, estimationResult[0].Estimation.Value);
            Assert.AreEqual(40.0, estimationResult[1].Estimation.Value);
        }

        private static async Task ProvideMessages(PlanningPokerSignalRClientFixture fixture, params Message[] messages)
        {
            var sentMessage = await fixture.GetSentMessage();
            Assert.IsNotNull(sentMessage);
            Assert.IsInstanceOfType(sentMessage, typeof(InvocationMessage));
            var sentInvocationMessage = (InvocationMessage)sentMessage;
            Assert.AreEqual(RequestName, sentInvocationMessage.Target);
            var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0L };
            CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, null, false);
            await fixture.ReceiveMessage(returnMessage);

            var notifyMessage = new InvocationMessage(ResponseName, new object[] { messages.ToList() });
            await fixture.ReceiveMessage(notifyMessage);
        }
    }
}
