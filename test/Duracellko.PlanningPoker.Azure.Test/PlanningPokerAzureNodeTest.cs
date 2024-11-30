using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Domain.Serialization;
using Duracellko.PlanningPoker.Domain.Test;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Azure.Test;

[TestClass]
[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Mock objects do not need to be disposed.")]
public class PlanningPokerAzureNodeTest
{
    private const string TeamName = "test team";
    private const string ScrumMasterName = "master";
    private const string MemberName = "member";
    private const string ObserverName = "observer";

    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    [TestMethod]
    public void Constructor_PlanningPoker_PlanningPokerIsSet()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

        // Act
        var result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, null);

        // Verify
        Assert.AreEqual<IAzurePlanningPoker>(planningPoker.Object, result.PlanningPoker);
    }

    [TestMethod]
    public void Constructor_Configuration_ConfigurationIsSet()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var configuration = CreateConfigutartion();

        // Act
        var result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, configuration);

        // Verify
        Assert.AreEqual<IAzurePlanningPokerConfiguration>(configuration, result.Configuration);
    }

    [TestMethod]
    public void Constructor_NoConfiguration_DefaultConfigurationIsSet()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

        // Act
        var result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, null);

        // Verify
        Assert.IsNotNull(result.Configuration);
    }

    [TestMethod]
    public void Constructor_PlanningPokerIsNull_ArgumentNullException()
    {
        // Arrange
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => CreatePlanningPokerAzureNode(null!, serviceBus.Object, null));
    }

    [TestMethod]
    public void Constructor_ServiceBusIsNull_ArgumentNullException()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => CreatePlanningPokerAzureNode(planningPoker.Object, null!, null));
    }

    [TestMethod]
    public async Task Start_TeamCreatedMessage_MessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var team = CreateBasicTeam();
        var teamLock = CreateTeamLock(team);
        var message = new ScrumTeamMessage(TeamName, MessageType.TeamCreated);
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);
        NodeMessage? nodeMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.TeamCreated)))
            .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.IsNotNull(nodeMessage);
        Assert.AreEqual<NodeMessageType>(NodeMessageType.TeamCreated, nodeMessage.MessageType);
        Assert.AreEqual<string?>(target.NodeId, nodeMessage.SenderNodeId);
        Assert.IsNotNull(nodeMessage.Data);
        Assert.IsInstanceOfType(nodeMessage.Data, typeof(byte[]));
        var expectedData = SerializeScrumTeam(team);
        CollectionAssert.AreEqual(expectedData, (byte[])nodeMessage.Data);
    }

    [TestMethod]
    public async Task Start_TeamCreatedMessageButTeamDoesNotExist_NoMessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var message = new ScrumTeamMessage(TeamName, MessageType.TeamCreated);
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).
            Throws(new ArgumentException("Team doesn't exist.")).Verifiable();
        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        serviceBus.Verify(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.TeamCreated)), Times.Never());
    }

    [TestMethod]
    public async Task Start_MemberJoined_MessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined) { MemberName = MemberName };
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);
        NodeMessage? nodeMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage)))
            .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(nodeMessage);
        Assert.AreEqual<NodeMessageType>(NodeMessageType.ScrumTeamMessage, nodeMessage.MessageType);
        Assert.AreEqual<string?>(target.NodeId, nodeMessage.SenderNodeId);
        Assert.AreEqual(message, nodeMessage.Data);
    }

    [TestMethod]
    public async Task Start_EstimationEnded_NoMessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var message = new ScrumTeamMessage(TeamName, MessageType.EstimationEnded);
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        serviceBus.Verify(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType != NodeMessageType.RequestTeamList)), Times.Never());
    }

    [TestMethod]
    public async Task Start_AvailableEstimationsChanged_MessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var deck = DeckProvider.Default.GetDeck(Deck.Rating).Select(e => e.Value).ToList();
        var message = new ScrumTeamEstimationSetMessage(TeamName, MessageType.AvailableEstimationsChanged) { Estimations = deck };
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);
        NodeMessage? nodeMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage)))
            .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(nodeMessage);
        Assert.AreEqual<NodeMessageType>(NodeMessageType.ScrumTeamMessage, nodeMessage.MessageType);
        Assert.AreEqual<string?>(target.NodeId, nodeMessage.SenderNodeId);
        Assert.AreEqual(message, nodeMessage.Data);
    }

    [TestMethod]
    public async Task Start_TimerStarted_MessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);
        NodeMessage? nodeMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage)))
            .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(nodeMessage);
        Assert.AreEqual<NodeMessageType>(NodeMessageType.ScrumTeamMessage, nodeMessage.MessageType);
        Assert.AreEqual<string?>(target.NodeId, nodeMessage.SenderNodeId);
        Assert.AreEqual(message, nodeMessage.Data);
    }

    [TestMethod]
    public async Task Start_FirstScrumTeamMessageFails_SecondMessageIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        var message1 = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined) { MemberName = MemberName };
        var message2 = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
        var startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message1, message2);

        var sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId);
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage && m.Data == message1)))
            .ThrowsAsync(new InvalidOperationException()).Verifiable();
        NodeMessage? nodeMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage && m.Data == message2)))
            .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

        // Act
        await target.Start();
        sendServiceBusMsg();
        startPlanningPokerMsg();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(nodeMessage);
        Assert.AreEqual<NodeMessageType>(NodeMessageType.ScrumTeamMessage, nodeMessage.MessageType);
        Assert.AreEqual<string?>(target.NodeId, nodeMessage.SenderNodeId);
        Assert.AreEqual(message2, nodeMessage.Data);
    }

    [TestMethod]
    public async Task Start_MemberJoinedFromServiceBus_MemberJoinedTeam()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var sessionId = Guid.NewGuid();
        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
        {
            MemberName = MemberName,
            MemberType = "Member",
            SessionId = sessionId
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        var observer = team.FindMemberOrObserver(MemberName);
        Assert.IsNotNull(observer);
        Assert.IsInstanceOfType(observer, typeof(Member));
        Assert.AreEqual<string>(MemberName, observer.Name);
        Assert.AreEqual<Guid>(sessionId, observer.SessionId);
    }

    [TestMethod]
    public async Task Start_NotInitAndMemberJoinedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
        {
            MemberName = MemberName,
            MemberType = "Member",
            SessionId = Guid.NewGuid()
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_ObserverJoinedFromServiceBus_ObserverJoinedTeam()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var sessionId = Guid.NewGuid();
        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
        {
            MemberName = ObserverName,
            MemberType = "Observer",
            SessionId = sessionId
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        var observer = team.FindMemberOrObserver(ObserverName);
        Assert.IsNotNull(observer);
        Assert.IsInstanceOfType(observer, typeof(Observer));
        Assert.AreEqual<string>(ObserverName, observer.Name);
        Assert.AreEqual<Guid>(sessionId, observer.SessionId);
    }

    [TestMethod]
    public async Task Start_MasterDisconnectedFromServiceBus_MasterIsDormant()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberDisconnected)
        {
            MemberName = ScrumMasterName,
            MemberType = "ScrumMaster"
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.IsTrue(team.ScrumMaster!.IsDormant);
    }

    [TestMethod]
    public async Task Start_NotInitAndMasterDisconnectedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberDisconnected)
        {
            MemberName = ScrumMasterName,
            MemberType = "ScrumMaster"
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_EstimationStartedFromServiceBus_TeamEstimationStarted()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
    }

    [TestMethod]
    public async Task Start_NotInitAndEstimationStartedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_EstimationCanceledFromServiceBus_TeamEstimationCanceled()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.EstimationCanceled);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        team.ScrumMaster!.StartEstimation();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual<TeamState>(TeamState.EstimationCanceled, team.State);
    }

    [TestMethod]
    public async Task Start_NotInitAndEstimationCanceledFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.EstimationCanceled);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_MasterEstimatedFromServiceBus_MasterEstimationIsSet()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberEstimationMessage(TeamName, MessageType.MemberEstimated)
        {
            MemberName = ScrumMasterName,
            Estimation = 5.0
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        team.ScrumMaster!.StartEstimation();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.IsNotNull(team.ScrumMaster.Estimation);
        Assert.AreEqual<double?>(5.0, team.ScrumMaster.Estimation.Value);
    }

    [TestMethod]
    public async Task Start_NotInitAndMasterEstimatedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberEstimationMessage(TeamName, MessageType.MemberEstimated)
        {
            MemberName = ScrumMasterName,
            Estimation = 5.0
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_AvailableEstimationsChangedFromServiceBus_AvailableEstimationsAreSet()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var deck = DeckProvider.Default.GetDeck(Deck.Tshirt).ToList();
        var message = new ScrumTeamEstimationSetMessage(TeamName, MessageType.AvailableEstimationsChanged)
        {
            Estimations = deck.Select(e => e.Value).ToList()
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var team = CreateBasicTeam();
        team.ScrumMaster!.StartEstimation();
        team.ScrumMaster.CancelEstimation();
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        CollectionAssert.AreEqual(deck, team.AvailableEstimations.ToList());
    }

    [TestMethod]
    public async Task Start_NotInitAndAvailableEstimationsChangedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var deck = DeckProvider.Default.GetDeck(Deck.Tshirt);
        var message = new ScrumTeamEstimationSetMessage(TeamName, MessageType.AvailableEstimationsChanged)
        {
            Estimations = deck.Select(e => e.Value).ToList()
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_MasterActivityFromServiceBus_MasterUpdatedActivityInTeam()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var sessionId = Guid.NewGuid();
        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberActivity)
        {
            MemberName = ScrumMasterName,
            MemberType = "ScrumMaster",
            SessionId = sessionId,
            AcknowledgedMessageId = 2
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 9, 9, 23, 27, 33, DateTimeKind.Utc));

        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = new ScrumTeam(TeamName, availableEstimations, dateTimeProvider, null);
        team.SetScrumMaster(ScrumMasterName);
        team.Join("member", true);
        team.Join("observer", false);
        var teamLock = SetupPlanningPoker(planningPoker, team);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 9, 9, 23, 28, 27, DateTimeKind.Utc));

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual<DateTime>(dateTimeProvider.UtcNow, team.ScrumMaster!.LastActivity);
        Assert.AreEqual<Guid>(sessionId, team.ScrumMaster.SessionId);
        Assert.AreEqual<long>(2, team.ScrumMaster.AcknowledgedMessageId);
        Assert.IsFalse(team.ScrumMaster.HasMessage);
        Assert.IsFalse(team.ScrumMaster.Messages.Any());
    }

    [TestMethod]
    public async Task Start_NotInitAndMasterActivityFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberActivity)
        {
            MemberName = ScrumMasterName,
            MemberType = "ScrumMaster",
            SessionId = Guid.NewGuid(),
            AcknowledgedMessageId = 2
        };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_TimerStartedFromServiceBus_TeamTimerStarted()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 16, 23, 22, 49, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual(endTime, team.TimerEndTime);
    }

    [TestMethod]
    public async Task Start_TimerStartedFromServiceBusAndTimerStartedInTeam_TeamTimerIsOverwritten()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 16, 23, 22, 49, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        var member = (Member)team.Join(MemberName, false);
        member.StartTimer(TimeSpan.FromMinutes(22));
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual(endTime, team.TimerEndTime);
    }

    [TestMethod]
    public async Task Start_TimerStartedFromServiceBusAndEndTimeIsInPast_TeamTimerIsNotStarted()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 16, 23, 50, 4, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.IsNull(team.TimerEndTime);
    }

    [TestMethod]
    public async Task Start_TimerStartedFromServiceBusAndAndTimeIsInPastAndTimerStartedInTeam_TeamTimerIsNotOverwritten()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 16, 23, 50, 4, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        team.ScrumMaster!.StartTimer(TimeSpan.FromMinutes(22));
        var teamLock = SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual(new DateTime(2021, 11, 17, 0, 12, 4, DateTimeKind.Utc), team.TimerEndTime);
    }

    [TestMethod]
    public async Task Start_NotInitAndTimerStartedFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);
        var message = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted) { EndTime = endTime };
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Start_TimerCanceledFromServiceBus_TeamTimerCanceled(bool timerStarted)
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.TimerCanceled);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 16, 23, 22, 49, DateTimeKind.Utc));
        var team = CreateBasicTeam(dateTimeProvider: dateTimeProvider);
        var teamLock = SetupPlanningPoker(planningPoker, team);
        if (timerStarted)
        {
            team.ScrumMaster!.StartTimer(TimeSpan.FromMinutes(1));
        }

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.IsNull(team.TimerEndTime);
    }

    [TestMethod]
    public async Task Start_NotInitAndTimerCanceledFromServiceBus_MessageIgnored()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message = new ScrumTeamMessage(TeamName, MessageType.TimerCanceled);
        var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_TeamCreatedFromServiceBus_TeamAttachedToPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var dateTimeProvider = new DateTimeProviderMock();
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion(), dateTimeProvider: dateTimeProvider);

        var nodeMessage = new NodeMessage(NodeMessageType.TeamCreated) { Data = CreateSerializedBasicTeam() };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        ScrumTeam? team = null;
        planningPoker.Setup(p => p.AttachScrumTeam(It.IsAny<ScrumTeam>()))
            .Callback<ScrumTeam>(t => team = t).Returns(Mock.Of<IScrumTeamLock>()).Verifiable();
        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(team);
        Assert.AreEqual<string>(TeamName, team.Name);
        Assert.AreEqual<DateTimeProvider>(dateTimeProvider, team.DateTimeProvider);
    }

    [TestMethod]
    public async Task Start_NotInitAndTeamCreatedFromServiceBus_IgnoreMessage()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var nodeMessage = new NodeMessage(NodeMessageType.TeamCreated) { Data = CreateSerializedBasicTeam() };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

        SetupPlanningPoker(planningPoker, null, true);
        planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify(p => p.AttachScrumTeam(It.IsAny<ScrumTeam>()), Times.Never());
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_2MessagesFromServiceBusAndFirstTeamDoesNotExist_SecondTeamEstimationStarted()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var message1 = new ScrumTeamMessage("No Team", MessageType.EstimationStarted);
        var nodeMessage1 = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message1 };
        var message2 = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
        var nodeMessage2 = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message2 };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage1, nodeMessage2);

        var team = CreateBasicTeam();
        var teamLock = SetupPlanningPoker(planningPoker, team);
        planningPoker.Setup(o => o.GetScrumTeam("No Team"))
            .Throws(new ArgumentException("Team doesn't exist.")).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
    }

    [TestMethod]
    public async Task Start_2TeamCreatedMessagesFromServiceBusAndFirstTeamExists_SecondTeamAttachedToPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var dateTimeProvider = new DateTimeProviderMock();
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion(), dateTimeProvider: dateTimeProvider);

        var existingTeam = CreateBasicTeam("Existing team");
        var existingTeamData = SerializeScrumTeam(existingTeam);
        var nodeMessage1 = new NodeMessage(NodeMessageType.TeamCreated) { Data = existingTeamData };
        var nodeMessage2 = new NodeMessage(NodeMessageType.TeamCreated) { Data = CreateSerializedBasicTeam() };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage1, nodeMessage2);

        ScrumTeam? team = null;
        planningPoker.Setup(p => p.AttachScrumTeam(It.Is<ScrumTeam>(t => t.Name == "Existing team")))
            .Throws(new InvalidOperationException()).Verifiable();
        planningPoker.Setup(p => p.AttachScrumTeam(It.Is<ScrumTeam>(t => t.Name == TeamName)))
            .Callback<ScrumTeam>(t => team = t).Returns(Mock.Of<IScrumTeamLock>()).Verifiable();
        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(team);
        Assert.AreEqual<string>(TeamName, team.Name);
        Assert.AreEqual<DateTimeProvider>(dateTimeProvider, team.DateTimeProvider);
    }

    [TestMethod]
    public async Task Start_TeamListMessageReceived_SetScrumTeamListOnPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, null);

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        IEnumerable<string>? initializationTeamList = null;
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(t => initializationTeamList = t).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(initializationTeamList);
        CollectionAssert.AreEquivalent(teamList, initializationTeamList.ToList());
    }

    [TestMethod]
    public async Task Start_TeamListMessageReceived_RequestForTeams()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, null);
        NodeMessage? requestTeamsMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.RequestTeams)))
            .Callback<NodeMessage>(m => requestTeamsMessage = m).Returns(Task.CompletedTask).Verifiable();

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>())).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(requestTeamsMessage);
        Assert.IsNotNull(requestTeamsMessage.Data);
        var requestedTeams = (string[])requestTeamsMessage.Data;
        CollectionAssert.AreEquivalent(teamList, requestedTeams);
    }

    [TestMethod]
    public async Task Start_InitializeTeamMessageReceived_InitializeTeamOnPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam) { Data = CreateSerializedBasicTeam() };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        ScrumTeam? initializingTeam = null;
        planningPoker.Setup(p => p.InitializeScrumTeam(It.IsAny<ScrumTeam>())).Callback<ScrumTeam>(t => initializingTeam = t).Verifiable();
        planningPoker.Setup(p => p.EndInitialization()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(initializingTeam);
        Assert.AreEqual<string>(TeamName, initializingTeam.Name);
    }

    [TestMethod]
    public async Task Start_InitializeTeamMessageReceivedButAnotherTeamIsNotInitializedYet_EndInitializationIsNotExecuted()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName, "team 2" };
        var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam) { Data = CreateSerializedBasicTeam() };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        planningPoker.Setup(p => p.InitializeScrumTeam(It.IsAny<ScrumTeam>())).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        planningPoker.Verify(p => p.EndInitialization(), Times.Never());
    }

    [TestMethod]
    public async Task Start_InitializeTeamMessageReceivedWithTeamNameOnly_SkipsInitializationOfThisTeam()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam)
        {
            Data = Encoding.UTF8.GetBytes("Deleted:" + TeamName)
        };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization()).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_RequestTeamListMessageReceived_TeamListIsObtainedFromPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList);
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var teamList = new string[] { TeamName };
        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        planningPoker.Setup(p => p.ScrumTeamNames).Returns(teamList).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
    }

    [TestMethod]
    public async Task Start_RequestTeamListMessageReceived_TeamListIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList) { SenderNodeId = "sender" };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
        NodeMessage? teamListMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.TeamList)))
            .Callback<NodeMessage>(m => teamListMessage = m).Returns(Task.CompletedTask).Verifiable();

        var teamList = new string[] { TeamName };
        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
        planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization());
        planningPoker.Setup(p => p.ScrumTeamNames).Returns(teamList);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(teamListMessage);
        Assert.IsNotNull(teamListMessage.Data);
        CollectionAssert.AreEquivalent(teamList, (string[])teamListMessage.Data);
        Assert.AreEqual<string?>(nodeMessage.SenderNodeId, teamListMessage.RecipientNodeId);
    }

    [TestMethod]
    public async Task Start_RequestTeamsMessageReceived_TeamIsObtainedFromPlanningPoker()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

        var teamLock = SetupPlanningPoker(planningPoker, CreateBasicTeam());

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        teamLock.Verify();
    }

    [TestMethod]
    public async Task Start_RequestTeamsMessageReceived_TeamIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
        NodeMessage? initializeTeamMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.InitializeTeam)))
            .Callback<NodeMessage>(m => initializeTeamMessage = m).Returns(Task.CompletedTask).Verifiable();

        var team = CreateBasicTeam();
        SetupPlanningPoker(planningPoker, team);

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(initializeTeamMessage);
        Assert.IsNotNull(initializeTeamMessage.Data);
        Assert.IsInstanceOfType(initializeTeamMessage.Data, typeof(byte[]));
        var expectedData = SerializeScrumTeam(team);
        CollectionAssert.AreEqual(expectedData, (byte[])initializeTeamMessage.Data);
        Assert.AreEqual<string?>(nodeMessage.SenderNodeId, initializeTeamMessage.RecipientNodeId);
    }

    [TestMethod]
    public async Task Start_RequestTeamsMessageReceivedButTeamDoesNotExistAnymore_TeamNameIsSentToServiceBus()
    {
        // Arrange
        var planningPoker = CreatePlanningPokerMock();
        var serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
        var target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

        var teamList = new string[] { TeamName };
        var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
        serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
        var sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
        NodeMessage? initializeTeamMessage = null;
        serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.InitializeTeam)))
            .Callback<NodeMessage>(m => initializeTeamMessage = m).Returns(Task.CompletedTask).Verifiable();

        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        planningPoker.Setup(p => p.EndInitialization()).Verifiable();
        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
        planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Throws(new ArgumentException("teamName")).Verifiable();

        // Act
        await target.Start();
        sendMessages();
        await target.Stop();

        // Verify
        planningPoker.Verify();
        serviceBus.Verify();
        Assert.IsNotNull(initializeTeamMessage);
        Assert.IsNotNull(initializeTeamMessage.Data);
        Assert.IsInstanceOfType(initializeTeamMessage.Data, typeof(byte[]));
        Assert.AreEqual<string>("Deleted:" + TeamName, Encoding.UTF8.GetString((byte[])initializeTeamMessage.Data));
        Assert.AreEqual<string?>(nodeMessage.SenderNodeId, initializeTeamMessage.RecipientNodeId);
    }

    private static PlanningPokerAzureNode CreatePlanningPokerAzureNode(
        IAzurePlanningPoker planningPoker,
        IServiceBus serviceBus,
        IAzurePlanningPokerConfiguration? configuration = null,
        ILogger<PlanningPokerAzureNode>? logger = null,
        DateTimeProvider? dateTimeProvider = null,
        GuidProvider? guidProvider = null)
    {
        if (logger == null)
        {
            logger = Mock.Of<ILogger<PlanningPokerAzureNode>>();
        }

        var serializer = new ScrumTeamSerializer(
            dateTimeProvider ?? new DateTimeProviderMock(),
            guidProvider ?? new GuidProviderMock());

        return new PlanningPokerAzureNode(planningPoker, serviceBus, configuration, serializer, logger);
    }

    private static Mock<IAzurePlanningPoker> CreatePlanningPokerMock()
    {
        var result = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
        result.SetupGet(p => p.GuidProvider).Returns(GuidProvider.Default);
        return result;
    }

    private static ScrumTeam CreateBasicTeam(string? name = null, DateTimeProvider? dateTimeProvider = null)
    {
        var result = new ScrumTeam(name ?? TeamName, null, dateTimeProvider, null);
        result.SetScrumMaster(ScrumMasterName);
        return result;
    }

    private static Mock<IScrumTeamLock> CreateTeamLock(ScrumTeam scrumTeam)
    {
        var result = new Mock<IScrumTeamLock>(MockBehavior.Strict);
        result.Setup(l => l.Team).Returns(scrumTeam);
        result.Setup(l => l.Lock()).Verifiable();
        result.Setup(l => l.Dispose()).Verifiable();
        return result;
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
    private static Action SetupServiceBus(Mock<IServiceBus> serviceBus, string nodeId, params NodeMessage[]? nodeMessages)
    {
        return SetupServiceBus(serviceBus, nodeId, null, nodeMessages);
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
    private static Action SetupServiceBus(Mock<IServiceBus> serviceBus, string nodeId, string[]? initializationTeamList, params NodeMessage[]? nodeMessages)
    {
        serviceBus.Setup(b => b.Register(nodeId)).Returns(Task.CompletedTask).Verifiable();
        serviceBus.Setup(b => b.Unregister()).Returns(Task.CompletedTask).Verifiable();

        var emptyTeamListMessage = new NodeMessage(NodeMessageType.TeamList)
        {
            Data = initializationTeamList ?? Array.Empty<string>(),
            RecipientNodeId = nodeId
        };

        var observableMessages = new Subject<NodeMessage>();
        serviceBus.Setup(b => b.ObservableMessages).Returns(observableMessages).Verifiable();
        if (initializationTeamList != null && initializationTeamList.Length != 0)
        {
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m =>
                m.MessageType == NodeMessageType.RequestTeamList || m.MessageType == NodeMessageType.RequestTeams)))
                .Returns(Task.CompletedTask).Verifiable();
        }
        else
        {
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.RequestTeamList)))
                .Returns(Task.CompletedTask).Verifiable();
        }

        return new Action(
            () =>
            {
                observableMessages.OnNext(emptyTeamListMessage);
                if (nodeMessages != null)
                {
                    foreach (var nodeMessage in nodeMessages)
                    {
                        observableMessages.OnNext(nodeMessage);
                    }
                }

                observableMessages.OnCompleted();
            });
    }

    [return: NotNullIfNotNull(nameof(team))]
    private static Mock<IScrumTeamLock>? SetupPlanningPoker(Mock<IAzurePlanningPoker> planningPoker, ScrumTeam? team, bool noEnd = false)
    {
        planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
        if (!noEnd)
        {
            planningPoker.Setup(p => p.EndInitialization()).Verifiable();
        }

        planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
        if (team != null)
        {
            var teamLock = CreateTeamLock(team);
            planningPoker.Setup(p => p.GetScrumTeam(team.Name)).Returns(teamLock.Object).Verifiable();
            return teamLock;
        }
        else
        {
            return null;
        }
    }

    private static Action SetupPlanningPokerMsg(Mock<IAzurePlanningPoker> planningPoker, params ScrumTeamMessage[] messages)
    {
        var observableMessages = new Subject<ScrumTeamMessage>();
        planningPoker.Setup(p => p.ObservableMessages).Returns(observableMessages).Verifiable();
        return new Action(
            () =>
            {
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        observableMessages.OnNext(message);
                    }
                }

                observableMessages.OnCompleted();
            });
    }

    private static byte[] CreateSerializedBasicTeam()
    {
        var team = CreateBasicTeam();
        return SerializeScrumTeam(team);
    }

    private static byte[] SerializeScrumTeam(ScrumTeam scrumTeam)
    {
        return JsonSerializer.SerializeToUtf8Bytes(scrumTeam.GetData(), _serializerOptions);
    }

    private static AzurePlanningPokerConfiguration CreateConfigutartion() => new AzurePlanningPokerConfiguration();
}
