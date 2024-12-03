using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Service;

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
        Assert.IsInstanceOfType<InvocationMessage>(sentMessage, out var sentInvocationMessage);
        Assert.AreEqual(RequestName, sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, sessionId, 0L };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        var notifyMessage = new InvocationMessage(ResponseName, [new List<Message>()]);
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
        Assert.IsInstanceOfType<InvocationMessage>(sentMessage, out var sentInvocationMessage);
        Assert.AreEqual(RequestName, sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, sessionId, 2157483849L };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var notifyMessage = new InvocationMessage(ResponseName, [new List<Message>()]);
        await fixture.ReceiveMessage(notifyMessage);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
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
            EstimationResult =
            [
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
            ]
        };
        await ProvideMessages(fixture, message);

        var result = await resultTask;
        await Task.Yield();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(message, result[0]);
        var estimationResult = ((EstimationResultMessage)result[0]).EstimationResult;
        Assert.AreEqual(2.0, estimationResult[0].Estimation!.Value);
        Assert.IsNull(estimationResult[1].Estimation!.Value);
        Assert.IsNull(estimationResult[2].Estimation);
        Assert.IsTrue(double.IsPositiveInfinity(estimationResult[3].Estimation!.Value!.Value));
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsAvailableEstimationsChangedMessage()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

        var message = new EstimationSetMessage
        {
            Id = 22,
            Type = MessageType.AvailableEstimationsChanged,
            Estimations =
            [
                new Estimation { Value = 0 },
                new Estimation { Value = 0.5 },
                new Estimation { Value = 1 },
                new Estimation { Value = 2 },
                new Estimation { Value = 3 },
                new Estimation { Value = 5 },
                new Estimation { Value = 100 },
                new Estimation { Value = Estimation.PositiveInfinity },
                new Estimation()
            ]
        };
        await ProvideMessages(fixture, message);

        var result = await resultTask;
        await Task.Yield();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(message, result[0]);
        var estimations = ((EstimationSetMessage)result[0]).Estimations;
        Assert.AreEqual(0.0, estimations[0].Value);
        Assert.AreEqual(0.5, estimations[1].Value);
        Assert.AreEqual(1.0, estimations[2].Value);
        Assert.AreEqual(2.0, estimations[3].Value);
        Assert.AreEqual(3.0, estimations[4].Value);
        Assert.AreEqual(5.0, estimations[5].Value);
        Assert.AreEqual(100.0, estimations[6].Value);
        Assert.IsTrue(double.IsPositiveInfinity(estimations[7].Value!.Value));
        Assert.IsNull(estimations[8].Value);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsTimerStartedMessage()
    {
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0, fixture.CancellationToken);

        var message = new TimerMessage
        {
            Id = 1,
            Type = MessageType.TimerStarted,
            EndTime = new DateTime(2021, 11, 17, 10, 3, 46, DateTimeKind.Utc)
        };
        await ProvideMessages(fixture, message);

        var result = await resultTask;
        await Task.Yield();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(message, result[0]);
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
            EstimationResult =
            [
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
            ]
        };
        await ProvideMessages(fixture, estimationStartedMessage, memberEstimatedMessage, estimationEndedMessage);

        var result = await resultTask;
        await Task.Yield();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(estimationStartedMessage, result[0]);
        Assert.AreEqual(memberEstimatedMessage, result[1]);
        Assert.AreEqual(estimationEndedMessage, result[2]);

        var estimationResult = ((EstimationResultMessage)result[2]).EstimationResult;
        Assert.AreEqual(5.0, estimationResult[0].Estimation!.Value);
        Assert.AreEqual(40.0, estimationResult[1].Estimation!.Value);
    }

    [TestMethod]
    public async Task GetMessages_HubException_UserDisconnectedException()
    {
        var sessionId = Guid.NewGuid();
        await using var fixture = new PlanningPokerSignalRClientFixture();

        var resultTask = fixture.Target.GetMessages(PlanningPokerData.TeamName, PlanningPokerData.MemberName, sessionId, 0, fixture.CancellationToken);

        var sentMessage = await fixture.GetSentMessage();
        Assert.IsNotNull(sentMessage);
        Assert.IsInstanceOfType<InvocationMessage>(sentMessage, out var sentInvocationMessage);
        Assert.AreEqual(RequestName, sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, sessionId, 0L };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var errorMessage = @"ArgumentException:{""Message"":""Invalid Session ID.""}";
        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, errorMessage, null, false);
        await fixture.ReceiveMessage(returnMessage);

        await Assert.ThrowsExceptionAsync<UserDisconnectedException>(() => resultTask);
    }

    private static async Task ProvideMessages(PlanningPokerSignalRClientFixture fixture, params Message[] messages)
    {
        var sentMessage = await fixture.GetSentMessage();
        Assert.IsNotNull(sentMessage);
        Assert.IsInstanceOfType<InvocationMessage>(sentMessage, out var sentInvocationMessage);
        Assert.AreEqual(RequestName, sentInvocationMessage.Target);
        var expectedArguments = new object[] { PlanningPokerData.TeamName, PlanningPokerData.MemberName, PlanningPokerData.SessionId, 0L };
        CollectionAssert.AreEqual(expectedArguments, sentInvocationMessage.Arguments);

        var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId!, null, null, false);
        await fixture.ReceiveMessage(returnMessage);

        var notifyMessage = new InvocationMessage(ResponseName, [messages.ToList()]);
        await fixture.ReceiveMessage(notifyMessage);
    }
}
