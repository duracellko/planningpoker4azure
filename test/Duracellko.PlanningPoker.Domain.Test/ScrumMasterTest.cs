using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class ScrumMasterTest
{
    [TestMethod]
    public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var name = "test";

        // Act
        var result = new ScrumMaster(team, name);

        // Verify
        Assert.AreEqual<ScrumTeam>(team, result.Team);
        Assert.AreEqual<string>(name, result.Name);
    }

    [TestMethod]
    public void Constructor_SessionId_ZeroGuid()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var name = "test";

        // Act
        var result = new ScrumMaster(team, name);

        // Verify
        Assert.AreEqual<Guid>(Guid.Empty, result.SessionId);
    }

    [TestMethod]
    public void Constructor_TeamNotSpecified_ArgumentNullException()
    {
        // Arrange
        var name = "test";

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => new ScrumMaster(null!, name));
    }

    [TestMethod]
    public void Constructor_NameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var team = new ScrumTeam("test team");

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => new ScrumMaster(team, string.Empty));
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_StateChangedToEstimationInProgress()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");

        // Act
        master.StartEstimation();

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_ScrumTeamGotMessageEstimationStarted()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_ScrumMasterGotMessageEstimationStarted()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_ScrumMasterReceivedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_MemberGotMessageEstimationStarted()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
    }

    [TestMethod]
    public void StartEstimation_MemberHasEstimation_MembersEstimationIsReset()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        member.Estimation = new Estimation(EstimationTestData.Unknown);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNull(member.Estimation);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_MemberReceivedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_ObserverGotMessageEstimationStarted()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", true);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_ObserverReceivedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", false);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartEstimation_EstimationInProgress_InvalidOperationException()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        Assert.ThrowsExactly<InvalidOperationException>(master.StartEstimation);
    }

    [TestMethod]
    public void StartEstimation_EstimationNotStarted_EstimationResultSetToNull()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNull(team.EstimationResult);
    }

    [TestMethod]
    public void StartEstimation_EstimationFinished_EstimationResultSetToNull()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        master.Estimation = new Estimation(EstimationTestData.Unknown);

        // Act
        master.StartEstimation();

        // Verify
        Assert.IsNull(team.EstimationResult);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_StateChangedToEstimationCanceled()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationCanceled, team.State);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ScrumTeamGetMessageEstimationCanceled()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ScrumTeamGet2Messages()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var eventArgsList = new List<MessageReceivedEventArgs>();
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
        master.StartEstimation();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.HasCount(2, eventArgsList);
        var message1 = eventArgsList[0].Message;
        var message2 = eventArgsList[1].Message;
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
    }

    [TestMethod]
    public void CancelEstimation_EstimationNotStarted_ScrumTeamGetNoMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsNull(eventArgs);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ScrumMasterGetMessageEstimationCanceled()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        master.ClearMessages();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ScrumMasterGet2Messages()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.AreEqual<int>(2, master.Messages.Count());
        var message1 = master.Messages.First();
        var message2 = master.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ScrumMasterMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void CancelEstimation_EstimationNotStarted_ScrumMasterGetNoMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsFalse(master.HasMessage);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_MemberGetMessageEstimationCanceled()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);
        master.StartEstimation();
        member.ClearMessages();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_MemberGet2Messages()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);
        master.StartEstimation();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.AreEqual<int>(2, member.Messages.Count());
        var message1 = member.Messages.First();
        var message2 = member.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_MemberMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);
        master.StartEstimation();
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void CancelEstimation_EstimationNotStarted_MemberGetNoMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = team.Join("member", false);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsFalse(member.HasMessage);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ObserverGetMessageEstimationCanceled()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", true);
        master.StartEstimation();
        observer.ClearMessages();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.First();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ObserverGet2Message()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", true);
        master.StartEstimation();

        // Act
        master.CancelEstimation();

        // Verify
        Assert.AreEqual<int>(2, observer.Messages.Count());
        var message1 = observer.Messages.First();
        var message2 = observer.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void CancelEstimation_EstimationInProgress_ObserverMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", true);
        master.StartEstimation();
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void CancelEstimation_EstimationNotStarted_ObserverGetNoMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var observer = team.Join("observer", true);

        // Act
        master.CancelEstimation();

        // Verify
        Assert.IsFalse(observer.HasMessage);
    }

    [TestMethod]
    public void CloseEstimation_2MembersWithoutVote_MemberEstimationsAreSetToNullEstimation()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var noVote1 = (Member)team.Join("no vote 1", false);
        var member = (Member)team.Join("member", false);
        var noVote2 = (Member)team.Join("no vote 2", false);
        master.StartEstimation();
        member.Estimation = new Estimation(EstimationTestData.Unknown);
        master.Estimation = new Estimation(0);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(noVote1.Estimation);
        Assert.IsNull(noVote1.Estimation.Value);
        Assert.IsNotNull(noVote2.Estimation);
        Assert.IsNull(noVote2.Estimation.Value);
        Assert.IsNotNull(master.Estimation);
        Assert.AreEqual<double?>(0, master.Estimation.Value);
        Assert.IsNotNull(member.Estimation);
        Assert.AreEqual<double?>(EstimationTestData.Unknown, member.Estimation.Value);
    }

    [TestMethod]
    public void CloseEstimation_NoVotes_MemberEstimationsAreSetToNullEstimation()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(master.Estimation);
        Assert.IsNull(master.Estimation.Value);
        Assert.IsNotNull(member.Estimation);
        Assert.IsNull(member.Estimation.Value);
    }

    [TestMethod]
    public void CloseEstimation_NoVotes_StateChangedToEstimationFinished()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        team.Join("no vote", false);
        master.StartEstimation();

        // Act
        master.CloseEstimation();

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationFinished, team.State);
    }

    [TestMethod]
    public void CloseEstimation_2MembersWithoutVote_EstimationResultIsGenerated()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var noVote1 = (Member)team.Join("no vote 1", false);
        var member = (Member)team.Join("member", false);
        var noVote2 = (Member)team.Join("no vote 2", false);
        master.StartEstimation();
        member.Estimation = new Estimation(0.5);
        master.Estimation = new Estimation(EstimationTestData.Infinity);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(team.EstimationResult);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, new Estimation(EstimationTestData.Infinity)),
            new(member, new Estimation(0.5)),
            new(noVote1, new Estimation()),
            new(noVote2, new Estimation())
        };
        CollectionAssert.AreEquivalent(expectedResult, team.EstimationResult.ToList());
    }

    [TestMethod]
    public void CloseEstimation_NoVotes_EstimationResultIsGenerated()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(team.EstimationResult);
        Assert.HasCount(1, team.EstimationResult);
        Assert.AreEqual<Estimation?>(new Estimation(), team.EstimationResult[master]);
    }

    [TestMethod]
    public void CloseEstimation_2MembersWithoutVote_ScrumTeamGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var noVote1 = (Member)team.Join("no vote 1", false);
        var member = (Member)team.Join("member", false);
        var noVote2 = (Member)team.Join("no vote 2", false);
        master.StartEstimation();
        member.Estimation = new Estimation(EstimationTestData.Unknown);
        master.Estimation = new Estimation(0);

        var eventArgsList = new List<MessageReceivedEventArgs>();
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

        // Act
        master.CloseEstimation();

        // Verify
        Assert.HasCount(3, eventArgsList);
        var message1 = eventArgsList[0].Message;
        var message2 = eventArgsList[1].Message;
        var message3 = eventArgsList[2].Message;
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message2.MessageType);
        var memberMessage1 = Assert.IsInstanceOfType<MemberMessage>(message1);
        var memberMessage2 = Assert.IsInstanceOfType<MemberMessage>(message2);

        var expectedMessageMembers = new Observer[] { noVote1, noVote2 };
        var actualMessageMembers = new Observer[] { memberMessage1.Member, memberMessage2.Member };
        CollectionAssert.AreEquivalent(expectedMessageMembers, actualMessageMembers);

        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message3.MessageType);
        var estimationEndedMessage = Assert.IsInstanceOfType<EstimationResultMessage>(message3);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, new Estimation(0)),
            new(member, new Estimation(EstimationTestData.Unknown)),
            new(noVote1, new Estimation()),
            new(noVote2, new Estimation())
        };
        CollectionAssert.AreEquivalent(expectedResult, estimationEndedMessage.EstimationResult.ToList());
    }

    [TestMethod]
    public void CloseEstimation_ScrumMasterWithoutVote_ScrumMasterGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        master.ClearMessages();

        // Act
        master.CloseEstimation();

        // Verify
        var masterMessages = master.Messages.ToList();
        Assert.HasCount(2, masterMessages);
        var message1 = masterMessages[0];
        var message2 = masterMessages[1];
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message1.MessageType);
        var memberMessage1 = Assert.IsInstanceOfType<MemberMessage>(message1);
        Assert.AreEqual<Observer>(master, memberMessage1.Member);

        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message2.MessageType);
        var estimationEndedMessage = Assert.IsInstanceOfType<EstimationResultMessage>(message2);
        Assert.HasCount(1, estimationEndedMessage.EstimationResult);
        Assert.AreEqual<Estimation?>(new Estimation(), estimationEndedMessage.EstimationResult[master]);
    }

    [TestMethod]
    public void CloseEstimation_2MembersWithoutVote_MemberGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var noVote1 = (Member)team.Join("no vote 1", false);
        var member = (Member)team.Join("member", false);
        var noVote2 = (Member)team.Join("no vote 2", false);
        master.StartEstimation();
        member.Estimation = new Estimation(EstimationTestData.Unknown);
        master.Estimation = new Estimation(0);

        member.ClearMessages();

        // Act
        master.CloseEstimation();

        // Verify
        var memberMessages = member.Messages.ToList();
        Assert.HasCount(3, memberMessages);
        var message1 = memberMessages[0];
        var message2 = memberMessages[1];
        var message3 = memberMessages[2];
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message2.MessageType);
        var memberMessage1 = Assert.IsInstanceOfType<MemberMessage>(message1);
        var memberMessage2 = Assert.IsInstanceOfType<MemberMessage>(message2);

        var expectedMessageMembers = new Observer[] { noVote1, noVote2 };
        var actualMessageMembers = new Observer[] { memberMessage1.Member, memberMessage2.Member };
        CollectionAssert.AreEquivalent(expectedMessageMembers, actualMessageMembers);

        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message3.MessageType);
        var estimationEndedMessage = Assert.IsInstanceOfType<EstimationResultMessage>(message3);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, new Estimation(0)),
            new(member, new Estimation(EstimationTestData.Unknown)),
            new(noVote1, new Estimation()),
            new(noVote2, new Estimation())
        };
        CollectionAssert.AreEquivalent(expectedResult, estimationEndedMessage.EstimationResult.ToList());
    }

    [TestMethod]
    public void CloseEstimation_MemberWithoutVote_MemberGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var noVote = (Member)team.Join("no vote", false);
        master.StartEstimation();
        member.Estimation = new Estimation(8);
        master.Estimation = new Estimation(3);

        noVote.ClearMessages();

        // Act
        master.CloseEstimation();

        // Verify
        var memberMessages = noVote.Messages.ToList();
        Assert.HasCount(2, memberMessages);
        var message1 = memberMessages[0];
        var message2 = memberMessages[1];
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message1.MessageType);
        var memberMessage1 = Assert.IsInstanceOfType<MemberMessage>(message1);
        Assert.AreEqual<Observer>(noVote, memberMessage1.Member);

        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message2.MessageType);
        var estimationEndedMessage = Assert.IsInstanceOfType<EstimationResultMessage>(message2);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, new Estimation(3)),
            new(member, new Estimation(8)),
            new(noVote, new Estimation())
        };
        CollectionAssert.AreEquivalent(expectedResult, estimationEndedMessage.EstimationResult.ToList());
    }

    [TestMethod]
    public void CloseEstimation_NoVotes_ScrumMasterMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();

        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void CloseEstimation_MemberWithoutVote_MemberMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var noVote = (Member)team.Join("no vote", false);
        master.StartEstimation();
        member.Estimation = new Estimation(8);
        master.Estimation = new Estimation(3);

        EventArgs? eventArgs = null;
        noVote.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void CloseEstimation_InitialState_StateIsNotChanged()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.AreEqual<TeamState>(TeamState.Initial, team.State);
    }

    [TestMethod]
    public void CloseEstimation_InitialState_EstimationsAreNotChanged()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        master.CloseEstimation();

        // Verify
        Assert.IsNull(master.Estimation);
        Assert.IsNull(member.Estimation);
    }

    [TestMethod]
    public void UpdateActivity_IsDormant_IsNotDormant()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        team.Join("observer", true);

        // Act
        team.Disconnect(master.Name);
        Assert.IsTrue(master.IsDormant);
        master.UpdateActivity();

        // Verify
        Assert.IsFalse(master.IsDormant);
    }
}
