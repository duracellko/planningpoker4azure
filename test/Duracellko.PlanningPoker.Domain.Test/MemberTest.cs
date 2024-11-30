using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class MemberTest
{
    private static readonly DateTime[] NowData = new[]
    {
        new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc),
        new DateTime(2022, 5, 4, 23, 23, 30, DateTimeKind.Utc),
        new DateTime(2019, 2, 14, 12, 0, 0, DateTimeKind.Utc),
    };

    private static readonly TimeSpan[] DurationData = new[]
    {
        new TimeSpan(1, 5, 45),
        new TimeSpan(0, 45, 0),
        new TimeSpan(0, 0, 1),
    };

    private static readonly DateTime[] ExpectedEndTimeData = new[]
    {
        new DateTime(2021, 11, 17, 10, 3, 46, DateTimeKind.Utc),
        new DateTime(2022, 5, 5, 0, 8, 30, DateTimeKind.Utc),
        new DateTime(2019, 2, 14, 12, 0, 1, DateTimeKind.Utc),
    };

    public static IEnumerable<object[]> TimerTestData => Enumerable.Range(0, 3)
        .Select(i => new object[] { NowData[i], DurationData[i], ExpectedEndTimeData[i] });

    [TestMethod]
    public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var name = "test";

        // Act
        var result = new Member(team, name);

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
        var result = new Member(team, name);

        // Verify
        Assert.AreEqual<Guid>(Guid.Empty, result.SessionId);
    }

    [TestMethod]
    public void Constructor_TeamNotSpecified_ArgumentNullException()
    {
        // Arrange
        var name = "test";

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => new Member(null!, name));
    }

    [TestMethod]
    public void Constructor_NameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var team = new ScrumTeam("test team");

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => new Member(team, string.Empty));
    }

    [TestMethod]
    public void Estimation_GetAfterConstruction_ReturnsNull()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var target = new Member(team, "test");

        // Act
        var result = target.Estimation;

        // Verify
        Assert.IsNull(result);
    }

    [TestMethod]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Property set is in Act section.")]
    public void Estimation_SetAndGet_ReturnsTheValue()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var estimation = new Estimation(2);
        var target = new Member(team, "test");

        // Act
        target.Estimation = estimation;
        var result = target.Estimation;

        // Verify
        Assert.AreEqual<Estimation>(estimation, result);
    }

    [TestMethod]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Property set is in Act section.")]
    public void Estimation_SetTwiceAndGet_ReturnsTheValue()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var estimation = new Estimation(21);
        var target = new Member(team, "test");

        // Act
        target.Estimation = estimation;
        target.Estimation = estimation;
        var result = target.Estimation;

        // Verify
        Assert.AreEqual<Estimation>(estimation, result);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_StateChangedToEstimationFinished()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(double.PositiveInfinity);
        var memberEstimation = new Estimation();

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationFinished, team.State);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_EstimationResultIsGenerated()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(8);
        var memberEstimation = new Estimation(20);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(team.EstimationResult);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, masterEstimation),
            new(member, memberEstimation),
        };
        CollectionAssert.AreEquivalent(expectedResult, team.EstimationResult.ToList());
    }

    [TestMethod]
    public void Estimation_SetOnAllMembersWithCustomValues_EstimationResultIsGenerated()
    {
        // Arrange
        var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(22.34);
        var memberEstimation = new Estimation(-100.2);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(team.EstimationResult);
        var expectedResult = new KeyValuePair<Member, Estimation>[]
        {
            new(master, masterEstimation),
            new(member, memberEstimation),
        };
        CollectionAssert.AreEquivalent(expectedResult, team.EstimationResult.ToList());
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_StateIsEstimationInProgress()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(89);

        // Act
        master.Estimation = masterEstimation;

        // Verify
        Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_EstimationResultIsNull()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var memberEstimation = new Estimation(100);

        // Act
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNull(team.EstimationResult);
    }

    [TestMethod]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Property set is in Act section.")]
    public void Estimation_SetTwiceToDifferentValues_EstimationIsChanged()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var estimation1 = new Estimation(8);
        var estimation2 = new Estimation(13);
        var target = new Member(team, "test");
        target.Estimation = estimation1;

        // Act
        target.Estimation = estimation2;

        // Verify
        Assert.AreEqual(estimation2, target.Estimation);
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_ScrumTeamGetMemberEstimatedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
        var memberEstimation = new Estimation(1);

        // Act
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_ScrumTeamGetEstimationEndedMessage()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
        var masterEstimation = new Estimation(55);
        var memberEstimation = new Estimation(55);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForScrumTeam()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
        var masterEstimation = new Estimation(20);
        var memberEstimation = new Estimation(3);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
        var estimationResultMessage = (EstimationResultMessage)message;
        Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_ScrumMasterGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        master.ClearMessages();
        var masterEstimation = new Estimation();
        var memberEstimation = new Estimation();

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(3, master.Messages.Count());
        var message = master.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForScrumMaster()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        master.ClearMessages();
        var masterEstimation = new Estimation(double.PositiveInfinity);
        var memberEstimation = new Estimation(double.PositiveInfinity);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(3, master.Messages.Count());
        var message = master.Messages.Last();
        Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
        var estimationResultMessage = (EstimationResultMessage)message;
        Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_ScrumMasterMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        master.ClearMessages();
        var masterEstimation = new Estimation(5);
        var memberEstimation = new Estimation(40);
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_ScrumMasterGetsMemberEstimatedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        master.ClearMessages();
        var memberEstimation = new Estimation(3);

        // Act
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(master.HasMessage);
        var message = master.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(MemberMessage));
        var memberMessage = (MemberMessage)message;
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_MemberGetEstimationEndedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(1);
        var memberEstimation = new Estimation();

        // Act
        master.Estimation = masterEstimation;
        member.ClearMessages();
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(2, member.Messages.Count());
        var message = member.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForMember()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        var masterEstimation = new Estimation(1);
        var memberEstimation = new Estimation(1);

        // Act
        master.Estimation = masterEstimation;
        member.ClearMessages();
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(2, member.Messages.Count());
        var message = member.Messages.Last();
        Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
        var estimationResultMessage = (EstimationResultMessage)message;
        Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_MemberMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        member.ClearMessages();
        var masterEstimation = new Estimation(5);
        var memberEstimation = new Estimation(8);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_MemberGetsMemberEstimatedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        member.ClearMessages();
        var memberEstimation = new Estimation(1);

        // Act
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(member.HasMessage);
        var message = member.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(MemberMessage));
        var memberMessage = (MemberMessage)message;
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_ObserverGetEstimationEndedMessage()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var observer = team.Join("observer", true);
        master.StartEstimation();
        var masterEstimation = new Estimation(21);
        var memberEstimation = new Estimation(34);

        // Act
        master.Estimation = masterEstimation;
        observer.ClearMessages();
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(2, observer.Messages.Count());
        var message = observer.Messages.Last();
        Assert.IsNotNull(message);
        Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForObserver()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var observer = team.Join("observer", true);
        master.StartEstimation();
        var masterEstimation = new Estimation(20);
        var memberEstimation = new Estimation(40);

        // Act
        master.Estimation = masterEstimation;
        observer.ClearMessages();
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(2, observer.Messages.Count());
        var message = observer.Messages.Last();
        Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
        var estimationResultMessage = (EstimationResultMessage)message;
        Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
    }

    [TestMethod]
    public void Estimation_SetOnAllMembers_ObserverMessageReceived()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var observer = team.Join("observer", true);
        master.StartEstimation();
        observer.ClearMessages();
        var masterEstimation = new Estimation(5);
        var memberEstimation = new Estimation(5);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.Estimation = masterEstimation;
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void Estimation_SetOnMemberOnly_ObserverGetsMemberEstimatedMessage()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var observer = team.Join("observer", true);
        master.StartEstimation();
        observer.ClearMessages();
        var memberEstimation = new Estimation();

        // Act
        member.Estimation = memberEstimation;

        // Verify
        Assert.IsTrue(observer.HasMessage);
        var message = observer.Messages.First();
        Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(MemberMessage));
        var memberMessage = (MemberMessage)message;
        Assert.AreEqual<Observer>(member, memberMessage.Member);
    }

    [TestMethod]
    public void Estimation_SetToNotAvailableValueWithStandardValues_ArgumentException()
    {
        // Arrange
        var team = new ScrumTeam("test team");
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        var masterEstimation = new Estimation(55.0);

        // Act
        Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
    }

    [TestMethod]
    public void Estimation_SetToNotAvailableValueWithFibonacciValues_ArgumentException()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        var masterEstimation = new Estimation(40.0);

        // Act
        Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
    }

    [TestMethod]
    public void Estimation_SetToNotAvailableValueWithCustomValues_ArgumentException()
    {
        // Arrange
        var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
        var master = team.SetScrumMaster("master");
        master.StartEstimation();
        var masterEstimation = new Estimation(double.PositiveInfinity);

        // Act
        Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
    }

    [DataTestMethod]
    [DynamicData(nameof(TimerTestData))]
    public void StartTimer_TimerNotStarted_TimerEndTimeIsSet(DateTime now, TimeSpan duration, DateTime expectedEndTime)
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(now);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        member.StartTimer(duration);

        // Assert
        Assert.AreEqual(expectedEndTime, team.TimerEndTime);
    }

    [DataTestMethod]
    [DynamicData(nameof(TimerTestData))]
    public void StartTimer_TimerIsStarted_TimerEndTimeIsOverwritten(DateTime now, TimeSpan duration, DateTime expectedEndTime)
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(now);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartEstimation();
        master.StartTimer(TimeSpan.FromMinutes(50));

        // Act
        member.StartTimer(duration);

        // Assert
        Assert.AreEqual(expectedEndTime, team.TimerEndTime);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_MemberGetTimerStartedMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[0]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        member.StartTimer(DurationData[0]);

        // Assert
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerStarted, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(TimerMessage));
        var timerMessage = (TimerMessage)message;
        Assert.AreEqual(ExpectedEndTimeData[0], timerMessage.EndTime);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_MemberMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[0]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        member.StartTimer(DurationData[0]);

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_ScrumMasterGetTimerStartedMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[1]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        master.ClearMessages();
        member.StartTimer(DurationData[1]);

        // Assert
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerStarted, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(TimerMessage));
        var timerMessage = (TimerMessage)message;
        Assert.AreEqual(ExpectedEndTimeData[1], timerMessage.EndTime);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_ScrumMasterMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[1]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        member.StartTimer(DurationData[1]);

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_ObserverGetTimerStartedMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[2]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartEstimation();
        var observer = team.Join("observer", true);

        // Act
        master.StartTimer(DurationData[2]);

        // Assert
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerStarted, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(TimerMessage));
        var timerMessage = (TimerMessage)message;
        Assert.AreEqual(ExpectedEndTimeData[2], timerMessage.EndTime);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_ObserverMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[2]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartEstimation();
        var observer = team.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.StartTimer(DurationData[2]);

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [TestMethod]
    public void StartTimer_DurationSpecified_ScrumTeamGetTimerStartedMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[0]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        member.StartTimer(DurationData[0]);

        // Assert
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.AreEqual<MessageType>(MessageType.TimerStarted, message.MessageType);
        Assert.IsInstanceOfType(message, typeof(TimerMessage));
        var timerMessage = (TimerMessage)message;
        Assert.AreEqual(ExpectedEndTimeData[0], timerMessage.EndTime);
    }

    [TestMethod]
    public void StartTimer_ZeroDuration_ArgumentOutOfRangeException()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[0]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => member.StartTimer(TimeSpan.Zero));
    }

    [TestMethod]
    public void StartTimer_NegativeDuration_ArgumentOutOfRangeException()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(NowData[0]);
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => member.StartTimer(TimeSpan.FromSeconds(-1)));
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_TimerEndTimeIsNull()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);

        // Act
        member.CancelTimer();

        // Assert
        Assert.IsNull(team.TimerEndTime);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsNotStarted_TimerEndTimeIsNull()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);

        // Act
        member.CancelTimer();

        // Assert
        Assert.IsNull(team.TimerEndTime);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_MemberGetTimerCanceledMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);

        // Act
        member.ClearMessages();
        member.CancelTimer();

        // Assert
        Assert.IsTrue(member.HasMessage);
        Assert.AreEqual(1, member.Messages.Count());
        var message = member.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerCanceled, message.MessageType);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_MemberMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);
        EventArgs? eventArgs = null;
        member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        member.CancelTimer();

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_ScrumMasterGetTimerCanceledMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);

        // Act
        master.ClearMessages();
        member.CancelTimer();

        // Assert
        Assert.IsTrue(master.HasMessage);
        Assert.AreEqual(1, master.Messages.Count());
        var message = master.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerCanceled, message.MessageType);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_ScrumMasterMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);
        EventArgs? eventArgs = null;
        master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        member.CancelTimer();

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_ObserverGetTimerCanceledMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartTimer(DurationData[0]);
        var observer = team.Join("observer", true);

        // Act
        master.CancelTimer();

        // Assert
        Assert.IsTrue(observer.HasMessage);
        Assert.AreEqual(1, observer.Messages.Count());
        var message = observer.Messages.Single();
        Assert.AreEqual<MessageType>(MessageType.TimerCanceled, message.MessageType);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_ObserverMessageReceived()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartTimer(DurationData[0]);
        var observer = team.Join("observer", true);
        EventArgs? eventArgs = null;
        observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

        // Act
        master.CancelTimer();

        // Assert
        Assert.IsNotNull(eventArgs);
    }

    [DataTestMethod]
    public void CancelTimer_TimerIsStarted_ScrumTeamGetTimerCanceledMessage()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProviderMock();
        var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        master.StartTimer(DurationData[0]);
        MessageReceivedEventArgs? eventArgs = null;
        team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

        // Act
        member.CancelTimer();

        // Assert
        Assert.IsNotNull(eventArgs);
        var message = eventArgs.Message;
        Assert.AreEqual<MessageType>(MessageType.TimerCanceled, message.MessageType);
    }
}
