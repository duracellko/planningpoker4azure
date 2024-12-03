﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class ScrumTeamTest
{
    [TestMethod]
    public void Constructor_TeamNameSpecified_TeamNameIsSet()
    {
        // Arrange
        var name = "test team";

        // Act
        var result = new ScrumTeam(name);

        // Verify
        Assert.AreEqual<string>(name, result.Name);
    }

    [TestMethod]
    public void Constructor_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var name = string.Empty;

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => new ScrumTeam(name));
    }

    [TestMethod]
    public void Observers_GetAfterConstruction_ReturnsEmptyCollection()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Observers;

        // Verify
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void Members_GetAfterConstruction_ReturnsEmptyCollection()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Members;

        // Verify
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void ScrumMaster_GetAfterConstruction_ReturnsNull()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.ScrumMaster;

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ScrumMaster_SetScrumMaster_ReturnsNewScrumMasterOfTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster(name);

        // Act
        var result = target.ScrumMaster;

        // Verify
        Assert.AreEqual<ScrumMaster?>(master, result);
    }

    [TestMethod]
    public void AvailableEstimations_Get_ReturnsPlanningPokerCardValues()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.AvailableEstimations;

        // Verify
        var expectedCollection = new double?[]
        {
            0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void AvailableEstimations_StandardEstimations_ReturnsPlanningPokerCardValues()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Standard);
        var target = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations, DateTimeProvider.Default);

        // Act
        var result = target.AvailableEstimations;

        // Verify
        var expectedCollection = new double?[]
        {
            0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void AvailableEstimations_FibonacciEstimations_ReturnsPlanningPokerCardValues()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var target = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations, DateTimeProvider.Default);

        // Act
        var result = target.AvailableEstimations;

        // Verify
        var expectedCollection = new double?[]
        {
            0.0, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 21.0, 34.0, 55.0, 89.0, double.PositiveInfinity, null
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void AvailableEstimations_ExplicitEstimations_ReturnsPlanningPokerCardValues()
    {
        // Arrange
        var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
        var target = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations, DateTimeProvider.Default);

        // Act
        var result = target.AvailableEstimations;

        // Verify
        var expectedCollection = new double?[]
        {
            99.0, -1.0, null, 22.34, -100.2
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void State_GetAfterConstruction_ReturnsInitial()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.State;

        // Verify
        Assert.AreEqual<TeamState>(TeamState.Initial, result);
    }

    [TestMethod]
    public void TimerEndTime_GetAfterConstruction_ReturnsNull()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.TimerEndTime;

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void EstimationResult_GetAfterConstruction_ReturnsNull()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.EstimationResult;

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void EstimationParticipants_GetAfterConstruction_ReturnsNull()
    {
        // Arrange
        var target = new ScrumTeam("test team");

        // Act
        var result = target.EstimationParticipants;

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void EstimationParticipants_EstimationStarted_ReturnsScrumMaster()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        var result = target.EstimationParticipants;

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<int>(1, result.Count());
        Assert.AreEqual<string>(master.Name, result.First().MemberName);
        Assert.IsFalse(result.First().Estimated);
    }

    [TestMethod]
    public void EstimationParticipants_MemberEstimated_MemberEstimatedButNotScrumMaster()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = target.Join("member", false);
        master.StartEstimation();

        // Act
        var result = target.EstimationParticipants;

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<int>(2, result.Count());

        var masterParticipant = result.First(p => p.MemberName == master.Name);
        Assert.IsNotNull(masterParticipant);
        Assert.IsFalse(masterParticipant.Estimated);

        var memberParticipant = result.First(p => p.MemberName == member.Name);
        Assert.IsNotNull(memberParticipant);
        Assert.IsFalse(memberParticipant.Estimated);
    }

    [TestMethod]
    public void SetScrumMaster_SetName_ReturnsNewScrumMasterOfTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");

        // Act
        var result = target.SetScrumMaster(name);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<ScrumTeam>(target, result.Team);
        Assert.AreEqual<string>(name, result.Name);
        Assert.IsFalse(result.IsDormant);
    }

    [TestMethod]
    public void SetScrumMaster_GuidProvider_ScrumMasterHasSessionId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidProvider = new GuidProviderMock(guid);
        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);

        // Act
        var result = target.SetScrumMaster(name);

        // Verify
        Assert.AreEqual<Guid>(guid, result.SessionId);
    }

    [TestMethod]
    public void SetScrumMaster_NoMembers_ScrumTeamGetsMemberJoinedMessage()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        var result = target.SetScrumMaster(name);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void SetScrumMaster_ObserverAlreadyJoined_ObserverGetsMemberJoinedMessage()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var observer = target.Join("observer", true);

        // Act
        var result = target.SetScrumMaster(name);

        // Verify
        Assert.AreEqual<int>(1, observer.Messages.Count());
        var message = observer.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void SetScrumMaster_ObserverAlreadyJoined_ObserverMessageReceived()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var observer = target.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.SetScrumMaster(name);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void SetScrumMaster_MemberAlreadyJoined_MemberGetsMemberJoinedMessage()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var member = target.Join("member", false);

        // Act
        var result = target.SetScrumMaster(name);

        // Verify
        Assert.AreEqual<int>(1, member.Messages.Count());
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void SetScrumMaster_MemberAlreadyJoined_MemberMessageReceived()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var member = target.Join("member", false);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.SetScrumMaster(name);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void SetScrumMaster_NameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var name = string.Empty;
        var target = new ScrumTeam("test team");

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => target.SetScrumMaster(name));
    }

    [TestMethod]
    public void SetScrumMaster_ScrumMasterIsAlreadySet_InvalidOperationException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => target.SetScrumMaster(name));
    }

    [TestMethod]
    public void SetScrumMaster_MemberWithSpecifiedNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join("test", false);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.SetScrumMaster(name));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void SetScrumMaster_ObserverWithSpecifiedNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join("test", true);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.SetScrumMaster(name));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void Join_SetNameAndNotIsObserver_ReturnsNewMemberOfTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Join(name, false);

        // Verify
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<Member>(result);
        Assert.AreEqual<ScrumTeam>(target, result.Team);
        Assert.AreEqual<string>(name, result.Name);
        Assert.IsFalse(result.IsDormant);
    }

    [TestMethod]
    public void Join_SetNameAndIsObserver_ReturnsNewObserverOfTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Join(name, true);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<Type>(typeof(Observer), result.GetType());
        Assert.AreEqual<ScrumTeam>(target, result.Team);
        Assert.AreEqual<string>(name, result.Name);
        Assert.IsFalse(result.IsDormant);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Join_GuidProvider_MemberHasSessionId(bool isObserver)
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidProvider = new GuidProviderMock(guid);
        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);

        // Act
        var result = target.Join(name, isObserver);

        // Verify
        Assert.AreEqual<Guid>(guid, result.SessionId);
    }

    [TestMethod]
    public void Join_NameIsEmpty_ArgumentNullEsception()
    {
        // Arrange
        var name = string.Empty;
        var target = new ScrumTeam("test team");

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => target.Join(name, false));
    }

    [TestMethod]
    public void Join_SetNameAndIsNotObserver_MemberIsInMembersCollection()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Join(name, false);

        // Verify
        CollectionAssert.AreEquivalent(new Observer[] { result }, target.Members.ToList());
    }

    [TestMethod]
    public void Join_SetNameAndIsNotObserverTwice_MembersAreInMembersCollection()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");

        // Act
        var result1 = target.Join(name1, false);
        var result2 = target.Join(name2, false);

        // Verify
        CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Members.ToList());
    }

    [TestMethod]
    public void Join_SetNameAndIsNotObserverTwice_ScrumTeamGets2Messages()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var eventArgsList = new List<MessageReceivedEventArgs>();
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

        // Act
        target.Join(name1, false);
        target.Join(name2, false);

        // Verify
        Assert.AreEqual<int>(2, eventArgsList.Count);
        var message1 = eventArgsList[0].Message;
        var message2 = eventArgsList[1].Message;
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
    }

    [TestMethod]
    public void Join_SetNameAndIsNotObserverTwice_ScrumMasterGets2Messages()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");

        // Act
        target.Join(name1, false);
        target.Join(name2, false);

        // Verify
        Assert.AreEqual<int>(2, master.Messages.Count());
        var message1 = master.Messages.First();
        var message2 = master.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void Join_SetNameAndIsObserver_ObserverIsInObserversCollection()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");

        // Act
        var result = target.Join(name, true);

        // Verify
        CollectionAssert.AreEquivalent(new Observer[] { result }, target.Observers.ToList());
    }

    [TestMethod]
    public void Join_SetNameAndIsObserverTwice_ObserversAreInObserversCollection()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");

        // Act
        var result1 = target.Join(name1, true);
        var result2 = target.Join(name2, true);

        // Verify
        CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Observers.ToList());
    }

    [TestMethod]
    public void Join_SetNameAndIsObserverTwice_ScrumTeamGets2Messages()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var eventArgsList = new List<MessageReceivedEventArgs>();
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

        // Act
        target.Join(name1, true);
        target.Join(name2, true);

        // Verify
        Assert.AreEqual<int>(2, eventArgsList.Count);
        var message1 = eventArgsList[0].Message;
        var message2 = eventArgsList[1].Message;
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
    }

    [TestMethod]
    public void Join_SetNameAndIsObserverTwice_ScrumMasterGets2Messages()
    {
        // Arrange
        var name1 = "test1";
        var name2 = "test2";
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");

        // Act
        target.Join(name1, true);
        target.Join(name2, true);

        // Verify
        Assert.AreEqual<int>(2, master.Messages.Count());
        var message1 = master.Messages.First();
        var message2 = master.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void Join_AsMemberAndMemberWithNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, false);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.Join(name, false));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void Join_AsMemberAndObserverWithNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, true);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.Join(name, false));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void Join_AsObserverAndMemberWithNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, false);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.Join(name, true));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void Join_AsObserverAndObserverWithNameExists_PlanningPokerException()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, true);

        // Act
        var exception = Assert.ThrowsException<PlanningPokerException>(() => target.Join(name, true));

        // Verify
        Assert.AreEqual("MemberAlreadyExists", exception.Error);
        Assert.AreEqual(name, exception.Argument);
    }

    [TestMethod]
    public void Join_EstimationStarted_OnlyScrumMasterIsInEstimationResult()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        master.StartEstimation();
        var masterEstimation = new Estimation();

        // Act
        target.Join("member", false);
        master.Estimation = masterEstimation;

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
        Assert.IsNotNull(target.EstimationResult);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, masterEstimation)
        };
        CollectionAssert.AreEquivalent(expectedResult, target.EstimationResult.ToList());
    }

    [TestMethod]
    public void Join_EstimationStarted_OnlyScrumMasterIsInEstimationParticipants()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        master.StartEstimation();

        // Act
        target.Join("member", false);
        var result = target.EstimationParticipants;

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<int>(1, result.Count());
        Assert.AreEqual<string>(master.Name, result.First().MemberName);
    }

    [TestMethod]
    public void Join_AsMember_ScrumMasterGetMemberJoinedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");

        // Act
        target.Join("member", false);

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
    }

    [TestMethod]
    public void Join_AsMember_ScrumTeamGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        var result = target.Join("member", false);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void Join_AsMember_ScrumMasterGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");

        // Act
        var result = target.Join("member", false);

        // Verify
        Assert.IsTrue(master.HasMessage);
        var message = master.Messages.First();
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void Join_AsMember_ScrumMasterMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.Join("member", false);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Join_AsMember_MemberDoesNotGetMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");

        // Act
        var result = target.Join("member", false);

        // Verify
        Assert.IsFalse(result.HasMessage);
    }

    [TestMethod]
    public void Join_AsMemberWhenObserverExists_ObserverGetMemberJoinedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);

        // Act
        target.Join("member", false);

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
    }

    [TestMethod]
    public void Join_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);

        // Act
        var result = target.Join("member", false);

        // Verify
        Assert.IsTrue(observer.HasMessage);
        var message = observer.Messages.First();
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(result, memberMessage.Member);
    }

    [TestMethod]
    public void Join_AsMemberWhenObserverExists_ObserverMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.Join("member", false);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Join_AsMemberWhenObserverExists_MemberDoesNotGetMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        target.Join("observer", true);

        // Act
        var result = target.Join("member", false);

        // Verify
        Assert.IsFalse(result.HasMessage);
    }

    [TestMethod]
    public void Disconnect_NameOfTheScrumMaster_ScrumMasterIsDormantInTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster(name);

        // Act
        target.Disconnect(name);

        // Verify
        CollectionAssert.AreEquivalent(new Member[] { master }, target.Members.ToList());
        Assert.IsTrue(master.IsDormant);
        Assert.AreEqual(master, target.ScrumMaster);
    }

    [TestMethod]
    public void Disconnect_NameOfTheMember_MemberIsRemovedFromTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, false);

        // Act
        target.Disconnect(name);

        // Verify
        Assert.IsFalse(target.Members.Any());
    }

    [TestMethod]
    public void Disconnect_NameOfTheObserver_ObserverIsRemovedFromTheTeam()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join(name, true);

        // Act
        target.Disconnect(name);

        // Verify
        Assert.IsFalse(target.Observers.Any());
    }

    [TestMethod]
    public void Disconnect_ObserverNorMemberWithTheNameDoNotExist_ObserversAndMembersAreUnchanged()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var observer = target.Join("observer", true);
        var member = target.Join("member", false);

        // Act
        target.Disconnect(name);

        // Verify
        CollectionAssert.AreEquivalent(new Observer[] { observer }, target.Observers.ToList());
        CollectionAssert.AreEquivalent(new Observer[] { master, member }, target.Members.ToList());
        Assert.IsFalse(master.IsDormant);
    }

    [TestMethod]
    public void Disconnect_EmptyName_ArgumentNullException()
    {
        // Arrange
        var name = string.Empty;
        var target = new ScrumTeam("test team");

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => target.Disconnect(name));
    }

    [TestMethod]
    public void Disconnect_EstimationStarted_OnlyScrumMasterIsInEstimationResult()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = (Member)target.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation();

        // Act
        target.Disconnect(member.Name);
        master.Estimation = masterEstimation;

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
        Assert.IsNotNull(target.EstimationResult);
        var expectedResult = new KeyValuePair<Member, Estimation?>[]
        {
            new(master, masterEstimation),
            new(member, null)
        };
        CollectionAssert.AreEquivalent(expectedResult, target.EstimationResult.ToList());
    }

    [TestMethod]
    public void Disconnect_ScrumMasterAfterEstimationStarted_EstimationIsInProgress()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = (Member)target.Join("member", false);
        master.StartEstimation();
        var memberEstimation = new Estimation();

        // Act
        member.Estimation = memberEstimation;
        target.Disconnect(master.Name);

        // Verify
        Assert.IsTrue(master.IsDormant);
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, target.State);
        Assert.IsNull(target.EstimationResult);
        Assert.IsNotNull(target.EstimationParticipants);
        Assert.AreEqual(2, target.EstimationParticipants.Count());
        Assert.IsTrue(target.EstimationParticipants.Any(e => e.MemberName == master.Name && !e.Estimated));
        Assert.IsTrue(target.EstimationParticipants.Any(e => e.MemberName == member.Name && e.Estimated));
    }

    [TestMethod]
    public void Disconnect_AsScrumMaster_ScrumTeamGetMemberDisconnectedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        target.Join("member", false);
        var eventArgsList = new List<MessageReceivedEventArgs>();
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

        // Act
        target.Disconnect(master.Name);
        target.Disconnect(master.Name);

        // Verify
        Assert.AreEqual(1, eventArgsList.Count);
        var eventArgs = eventArgsList[0];
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsScrumMaster_ScrumTeamGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        target.Join("member", false);
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        target.Disconnect(master.Name);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(master, memberMessage.Member);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumTeamGetMemberDisconnectedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumTeamGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumTeamGet2Messages()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var eventArgsList = new List<MessageReceivedEventArgs>();
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
        var member = target.Join("member", false);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.AreEqual<int>(2, eventArgsList.Count);
        var message1 = eventArgsList[0].Message;
        var message2 = eventArgsList[1].Message;
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumMasterGetMemberDisconnectedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = target.Join("member", false);
        master.ClearMessages();

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumMasterGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = target.Join("member", false);
        master.ClearMessages();

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(master.HasMessage);
        var message = master.Messages.First();
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumMasterGet2Messages()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = target.Join("member", false);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.AreEqual<int>(2, master.Messages.Count());
        var message1 = master.Messages.First();
        var message2 = master.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void Disconnect_AsMember_ScrumMasterMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        var member = target.Join("member", false);
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Disconnect_AsMember_MemberGetsEmptyMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMemberWhenObserverExists_ObserverGetMemberDisconnectedMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        var observer = target.Join("observer", true);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        var observer = target.Join("observer", true);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(observer.HasMessage);
        var message = observer.Messages.First();
        Assert.IsInstanceOfType<MemberMessage>(message, out var memberMessage);
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Disconnect_AsMemberWhenObserverExists_ObserverMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        var observer = target.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Disconnect_AsMemberWhenObserverExists_MemberGetsEmptyMessage()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        target.Join("observer", true);
        member.ClearMessages();

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
    }

    [TestMethod]
    public void Disconnect_AsMemberWhenObserverExists_ObserverGet2Messages()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);
        var member = target.Join("member", false);

        // Act
        target.Disconnect(member.Name);

        // Verify
        Assert.AreEqual<int>(2, observer.Messages.Count());
        var message1 = observer.Messages.First();
        var message2 = observer.Messages.Skip(1).First();
        Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
        Assert.AreEqual<long>(1, message1.Id);
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
        Assert.AreEqual<long>(2, message2.Id);
    }

    [TestMethod]
    public void FindMemberOrObserver_ObserverExists_ReturnsObserver()
    {
        // Arrange
        var name = "observer2";
        var guidProvider = new GuidProviderMock();
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);
        guidProvider.SetGuid(Guid.NewGuid());

        // Act
        var result = target.FindMemberOrObserver(name);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<string>(name, result.Name);
        Assert.AreEqual<Guid>(GuidProviderMock.DefaultGuid, result.SessionId);
    }

    [TestMethod]
    public void FindMemberOrObserver_MemberExists_ReturnsMember()
    {
        // Arrange
        var name = "member2";
        var guidProvider = new GuidProviderMock();
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);
        guidProvider.SetGuid(Guid.NewGuid());

        // Act
        var result = target.FindMemberOrObserver(name);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<string>(name, result.Name);
        Assert.AreEqual<Guid>(GuidProviderMock.DefaultGuid, result.SessionId);
    }

    [TestMethod]
    public void FindMemberOrObserver_MemberNorObserverExists_ReturnsNull()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);

        // Act
        var result = target.FindMemberOrObserver(name);

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateSession_ObserverExists_ReturnsObserver()
    {
        // Arrange
        var name = "observer2";
        var guidProvider = new GuidProviderMock();
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);
        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.CreateSession(name);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<string>(name, result.Name);
        Assert.AreEqual<Guid>(guid, result.SessionId);
    }

    [TestMethod]
    public void CreateSession_MemberExists_ReturnsMember()
    {
        // Arrange
        var name = "member2";
        var guidProvider = new GuidProviderMock();
        var target = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: guidProvider);
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);
        var guid = Guid.NewGuid();
        guidProvider.SetGuid(guid);

        // Act
        var result = target.CreateSession(name);

        // Verify
        Assert.IsNotNull(result);
        Assert.AreEqual<string>(name, result.Name);
        Assert.AreEqual<Guid>(guid, result.SessionId);
    }

    [TestMethod]
    public void CreateSession_MemberNorObserverExists_ReturnsNull()
    {
        // Arrange
        var name = "test";
        var target = new ScrumTeam("test team");
        target.Join("observer1", true);
        target.Join("observer2", true);
        target.Join("member1", false);
        target.Join("member2", false);

        // Act
        var result = target.CreateSession(name);

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_RatingDeck_AvailableEstimationsAreSet()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.Rating);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.AreEqual(newEstimationsSet, target.AvailableEstimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_DefaultDeck_AvailableEstimationsAreNotChanged()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var newEstimationsSet = DeckProvider.Default.GetDefaultDeck();

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.AreEqual(newEstimationsSet, target.AvailableEstimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_EstimationIsFinished_AvailableEstimationsAreSet()
    {
        // Arrange
        var target = ScrumTeamTestData.CreateScrumTeam("test team", DeckProvider.Default.GetDeck(Deck.Rating));
        var master = target.SetScrumMaster("master");
        var member = (Member)target.Join("member", false);
        master.StartEstimation();
        member.Estimation = new Estimation(2);
        master.Estimation = new Estimation(10);

        var newEstimationsSet = ScrumTeamTestData.GetCustomEstimationDeck();

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.AreEqual(newEstimationsSet, target.AvailableEstimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_EstimationIsCanceled_AvailableEstimationsAreSet()
    {
        // Arrange
        var target = ScrumTeamTestData.CreateScrumTeam("test team", DeckProvider.Default.GetDeck(Deck.Tshirt));
        var master = target.SetScrumMaster("master");
        var member = (Member)target.Join("member", false);
        master.StartEstimation();
        member.Estimation = new Estimation(-999506.0);
        master.CancelEstimation();

        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.RockPaperScissorsLizardSpock);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.AreEqual(newEstimationsSet, target.AvailableEstimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_EstimationIsStarted_InvalidOperationException()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        master.StartEstimation();
        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.Rating);

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => target.ChangeAvailableEstimations(newEstimationsSet));
    }

    [TestMethod]
    public void ChangeAvailableEstimations_Null_ArgumentNullException()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => target.ChangeAvailableEstimations(null!));
    }

    [TestMethod]
    public void ChangeAvailableEstimations_RatingDeck_ScrumTeamGotMessageAvailableEstimationsChanged()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        master.StartEstimation();
        master.Estimation = new Estimation(20.0);

        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.Rating);
        MessageReceivedEventArgs? eventArgs = null;
        target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.AvailableEstimationsChanged, message.MessageType);
        Assert.IsInstanceOfType<EstimationSetMessage>(message, out var estimationSetMessage);
        Assert.AreEqual(newEstimationsSet, estimationSetMessage.Estimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_RatingDeck_ScrumMasterGetMessageAvailableEstimationsChanged()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        target.Join("member", false);
        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.Rating);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(2, master.Messages.Count());
        var message = master.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.AvailableEstimationsChanged, message.MessageType);
        Assert.IsInstanceOfType<EstimationSetMessage>(message, out var estimationSetMessage);
        Assert.AreEqual(newEstimationsSet, estimationSetMessage.Estimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_RatingDeck_ScrumMasterMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        var master = target.SetScrumMaster("master");
        target.Join("member", false);

        var newEstimationsSet = DeckProvider.Default.GetDeck(Deck.Rating);
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_CustomEstimationDeck_MemberGetMessageAvailableEstimationsChanged()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);
        var newEstimationsSet = ScrumTeamTestData.GetCustomEstimationDeck();

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.AvailableEstimationsChanged, message.MessageType);
        Assert.IsInstanceOfType<EstimationSetMessage>(message, out var estimationSetMessage);
        Assert.AreEqual(newEstimationsSet, estimationSetMessage.Estimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_CustomEstimationDeck_MemberMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var member = target.Join("member", false);

        var newEstimationsSet = ScrumTeamTestData.GetCustomEstimationDeck();
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_DefaultEstimationDeck_ObserverGetMessageAvailableEstimationsChanged()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);
        var newEstimationsSet = DeckProvider.Default.GetDefaultDeck();

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.AvailableEstimationsChanged, message.MessageType);
        Assert.IsInstanceOfType<EstimationSetMessage>(message, out var estimationSetMessage);
        Assert.AreEqual(newEstimationsSet, estimationSetMessage.Estimations);
    }

    [TestMethod]
    public void ChangeAvailableEstimations_DefaultEstimationDeck_ObserverMessageReceived()
    {
        // Arrange
        var target = new ScrumTeam("test team");
        target.SetScrumMaster("master");
        var observer = target.Join("observer", true);

        var newEstimationsSet = DeckProvider.Default.GetDefaultDeck();
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        target.ChangeAvailableEstimations(newEstimationsSet);

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_ScrumMasterIsActive_TeamIsUnchanged()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = target.SetScrumMaster(name);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(1, target.Members.Count());
        Assert.AreEqual(master, target.ScrumMaster);
        Assert.IsFalse(master.IsDormant);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_ScrumMasterIsNotActive_ScrumMasterIsDormant()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = target.SetScrumMaster(name);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(1, target.Members.Count());
        Assert.AreEqual(master, target.ScrumMaster);
        Assert.IsTrue(master.IsDormant);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_NoInactiveMembers_TeamIsUnchanged()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.Join(name, false);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(1, target.Members.Count());
    }

    [TestMethod]
    public void DisconnectInactiveObservers_InactiveMember_MemberIsDisconnected()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.Join(name, false);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(0, target.Members.Count());
    }

    [TestMethod]
    public void DisconnectInactiveObservers_NoInactiveObservers_TeamIsUnchanged()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.Join(name, true);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(1, target.Observers.Count());
    }

    [TestMethod]
    public void DisconnectInactiveObservers_InactiveObserver_ObserverIsDisconnected()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var name = "test";
        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.Join(name, true);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<int>(0, target.Observers.Count());
    }

    [TestMethod]
    public void DisconnectInactiveObservers_ActiveMemberAndInactiveScrumMaster_MemberMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.SetScrumMaster("master");

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30, DateTimeKind.Utc));
        var member = target.Join("member", false);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.IsNotNull(eventArgs);
        Assert.IsTrue(member.HasMessage);
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_ActiveMemberAndInactiveObserver_MemberMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.SetScrumMaster("master");
        target.Join("observer", true);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30, DateTimeKind.Utc));
        var member = target.Join("member", false);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.IsNotNull(eventArgs);
        Assert.IsTrue(member.HasMessage);
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_ActiveObserverAndInactiveMember_ObserverMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        target.SetScrumMaster("master");
        target.Join("member", false);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30, DateTimeKind.Utc));
        var observer = target.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.IsNotNull(eventArgs);
        Assert.IsTrue(observer.HasMessage);
        var message = observer.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_EstimationStartedActiveScrumMasterInactiveMember_ScrumMasterGetsEstimationResult()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = target.SetScrumMaster("master");
        target.Join("member", false);
        master.StartEstimation();

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30, DateTimeKind.Utc));
        master.Estimation = new Estimation();
        master.UpdateActivity();

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));
        master.ClearMessages();

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
        Assert.IsNotNull(target.EstimationResult);

        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(2, master.Messages.Count());
        var message = master.Messages.Last();
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
    }

    [TestMethod]
    public void DisconnectInactiveObservers_EstimationStartedActiveMemberInactiveScrumMaster_EstimationIsInProgress()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20, DateTimeKind.Utc));

        var target = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = target.SetScrumMaster("master");
        var member = (Member)target.Join("member", false);
        master.StartEstimation();

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30, DateTimeKind.Utc));
        member.Estimation = new Estimation();
        member.UpdateActivity();

        dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55, DateTimeKind.Utc));
        master.ClearMessages();

        // Act
        target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

        // Verify
        Assert.IsTrue(master.IsDormant);
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, target.State);
        Assert.IsNull(target.EstimationResult);
        Assert.IsNotNull(target.EstimationParticipants);
        Assert.AreEqual(2, target.EstimationParticipants.Count());
        Assert.IsTrue(target.EstimationParticipants.Any(e => e.MemberName == master.Name && !e.Estimated));
        Assert.IsTrue(target.EstimationParticipants.Any(e => e.MemberName == member.Name && e.Estimated));
    }
}
