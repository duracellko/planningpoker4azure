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
public class PlanningPokerClientTestMessages
{
    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_RequestsGetMessagesUrl()
    {
        var sessionId = Guid.NewGuid();
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ScrumMasterName}&sessionId={sessionId}&lastMessageId=0")
            .Respond(PlanningPokerClientTest.JsonType, "[]");
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, sessionId, 0, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task GetMessages_LastMessageId_RequestsGetMessagesUrl()
    {
        var sessionId = Guid.NewGuid();
        var httpMock = new MockHttpMessageHandler();
        httpMock.Expect(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&sessionId={sessionId}&lastMessageId=2157483849")
            .Respond(PlanningPokerClientTest.JsonType, "[]");
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, sessionId, 2157483849, CancellationToken.None);

        httpMock.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsEmptyMessage()
    {
        var messageJson = PlanningPokerClientData.GetEmptyMessageJson();
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = result[0];
        Assert.AreEqual(0, message.Id);
        Assert.AreEqual(MessageType.Empty, message.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsMemberJoinedMessage()
    {
        var messageJson = PlanningPokerClientData.GetMemberJoinedMessageJson("1");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<MemberMessage>(result[0]);
        Assert.AreEqual(1, message.Id);
        Assert.AreEqual(MessageType.MemberJoined, message.Type);
        Assert.IsNotNull(message.Member);
        Assert.AreEqual(PlanningPokerClientData.MemberName, message.Member.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, message.Member.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsMemberDisconnectedMessage()
    {
        var messageJson = PlanningPokerClientData.GetMemberDisconnectedMessageJson("2", name: PlanningPokerClientData.ObserverName, type: PlanningPokerClientData.ObserverType);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<MemberMessage>(result[0]);
        Assert.AreEqual(2, message.Id);
        Assert.AreEqual(MessageType.MemberDisconnected, message.Type);
        Assert.IsNotNull(message.Member);
        Assert.AreEqual(PlanningPokerClientData.ObserverName, message.Member.Name);
        Assert.AreEqual(PlanningPokerClientData.ObserverType, message.Member.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsEstimationStartedMessage()
    {
        var messageJson = PlanningPokerClientData.GetEstimationStartedMessageJson("2157483849");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = result[0];
        Assert.AreEqual(2157483849, message.Id);
        Assert.AreEqual(MessageType.EstimationStarted, message.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsEstimationEndedMessage()
    {
        var messageJson = PlanningPokerClientData.GetEstimationEndedMessageJson("8");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<EstimationResultMessage>(result[0]);
        Assert.AreEqual(8, message.Id);
        Assert.AreEqual(MessageType.EstimationEnded, message.Type);

        Assert.HasCount(4, message.EstimationResult);
        var estimationResult = message.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(2.0, estimationResult.Estimation!.Value);

        estimationResult = message.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation!.Value);

        estimationResult = message.EstimationResult[2];
        Assert.AreEqual("Me", estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsNull(estimationResult.Estimation);

        estimationResult = message.EstimationResult[3];
        Assert.AreEqual(PlanningPokerClientData.ObserverName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimation!.Value!.Value));
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsEstimationCanceledMessage()
    {
        var messageJson = PlanningPokerClientData.GetEstimationCanceledMessageJson("123");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = result[0];
        Assert.AreEqual(123, message.Id);
        Assert.AreEqual(MessageType.EstimationCanceled, message.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsMemberEstimatedMessage()
    {
        var messageJson = PlanningPokerClientData.GetMemberEstimatedMessageJson("22");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<MemberMessage>(result[0]);
        Assert.AreEqual(22, message.Id);
        Assert.AreEqual(MessageType.MemberEstimated, message.Type);
        Assert.IsNotNull(message.Member);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, message.Member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, message.Member.Type);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_ReturnsAvailableEstimationsChangedMessage()
    {
        var messageJson = PlanningPokerClientData.GetAvailableEstimationsChangedMessageJson("8");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<EstimationSetMessage>(result[0]);
        Assert.AreEqual(8, message.Id);
        Assert.AreEqual(MessageType.AvailableEstimationsChanged, message.Type);
        var estimations = message.Estimations;
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
        var messageJson = PlanningPokerClientData.GetTimerStartedMessageJson("1");
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(1, result);
        var message = Assert.IsInstanceOfType<TimerMessage>(result[0]);
        Assert.AreEqual(1, message.Id);
        Assert.AreEqual(MessageType.TimerStarted, message.Type);
        Assert.AreEqual(new DateTime(2021, 11, 17, 10, 3, 46, DateTimeKind.Utc), message.EndTime);
    }

    [TestMethod]
    public async Task GetMessages_TeamAndMemberName_Returns3Messages()
    {
        var estimationStartedMessageJson = PlanningPokerClientData.GetEstimationStartedMessageJson("8");
        var memberEstimatedMessageJson = PlanningPokerClientData.GetMemberEstimatedMessageJson("9");
        var estimationEndedMessageJson = PlanningPokerClientData.GetEstimationEndedMessage2Json("10");
        var json = PlanningPokerClientData.GetMessagesJson(estimationStartedMessageJson, memberEstimatedMessageJson, estimationEndedMessageJson);
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(PlanningPokerClientTest.JsonType, json);
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        var result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None);

        Assert.HasCount(3, result);
        var message = result[0];
        Assert.AreEqual(8, message.Id);
        Assert.AreEqual(MessageType.EstimationStarted, message.Type);

        var memberMessage = Assert.IsInstanceOfType<MemberMessage>(result[1]);
        Assert.AreEqual(9, memberMessage.Id);
        Assert.AreEqual(MessageType.MemberEstimated, memberMessage.Type);
        Assert.IsNotNull(memberMessage.Member);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, memberMessage.Member.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, memberMessage.Member.Type);

        var estimationMessage = Assert.IsInstanceOfType<EstimationResultMessage>(result[2]);
        Assert.AreEqual(10, estimationMessage.Id);
        Assert.AreEqual(MessageType.EstimationEnded, estimationMessage.Type);

        Assert.HasCount(2, estimationMessage.EstimationResult);
        var estimationResult = estimationMessage.EstimationResult[0];
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
        Assert.AreEqual(5.0, estimationResult.Estimation!.Value);

        estimationResult = estimationMessage.EstimationResult[1];
        Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member!.Name);
        Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
        Assert.AreEqual(40.0, estimationResult.Estimation!.Value);
    }

    [TestMethod]
    public async Task GetMessages_NotFoundResponse_UserDisconnectedException()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(HttpStatusCode.NotFound, "text/plain", "Invalid Session ID.");
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        await Assert.ThrowsExactlyAsync<UserDisconnectedException>(() => target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None));
    }

    [TestMethod]
    public async Task GetMessages_BadRequestResponse_UserDisconnectedException()
    {
        var httpMock = new MockHttpMessageHandler();
        httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Team does not exist.");
        var target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

        await Assert.ThrowsExactlyAsync<UserDisconnectedException>(() => target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, Guid.NewGuid(), 0, CancellationToken.None));
    }
}
