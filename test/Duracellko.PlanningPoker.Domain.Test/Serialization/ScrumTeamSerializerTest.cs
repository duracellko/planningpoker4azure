using System;
using System.IO;
using System.Linq;
using Duracellko.PlanningPoker.Domain.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test.Serialization;

[TestClass]
public class ScrumTeamSerializerTest
{
    [TestMethod]
    public void SerializeAndDeserialize_EmptyTeam_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_FibonacciEstimations_CopyOfTheTeam()
    {
        // Arrange
        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = ScrumTeamTestData.CreateScrumTeam("test", availableEstimations: availableEstimations);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_CustomEstimations_CopyOfTheTeam()
    {
        // Arrange
        var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
        var team = ScrumTeamTestData.CreateScrumTeam("test", availableEstimations: availableEstimations);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TeamWithScrumMaster_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TeamWithMember_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");
        team.Join("member", false);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TeamWithObserver_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");
        team.Join("member", true);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TeamWithDormantScrumMaster_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");
        team.Join("member", false);
        team.Disconnect("master");

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TeamWithMembersAndObservers_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");
        team.Join("member", false);
        team.Join("observer", true);
        team.Join("Bob", true);
        team.Join("Alice", false);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_EstimationStarted_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartEstimation();

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_EstimationInProgress_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        var member = (Member)team.Join("Bob", false);
        team.Join("observer", true);
        master.StartEstimation();
        team.Join("Alice", false);
        member.Estimation = new Estimation();

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_MemberEstimated_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        var member = (Member)team.Join("Bob", false);
        team.Join("observer", true);
        master.StartEstimation();
        team.Join("Alice", false);
        member.Estimation = team.AvailableEstimations.Single(e => e.Value == 8);
        master.Estimation = team.AvailableEstimations.Single(e => e.Value == EstimationTestData.Unknown);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_EstimationEnded_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        team.Join("observer", true);
        master.StartEstimation();
        team.Join("Bob", true);
        team.Join("Alice", false);
        member.Estimation = team.AvailableEstimations.Single(e => e.Value == 0.5);
        master.Estimation = team.AvailableEstimations.Single(e => e.Value == EstimationTestData.Infinity);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_EstimationEndedWithNoVote_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        team.Join("observer", true);
        master.StartEstimation();
        team.Join("Bob", true);
        team.Join("Alice", false);
        master.Estimation = team.AvailableEstimations.Single(e => e.Value == 0);
        member.Estimation = new Estimation();

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_EstimationIsCancelled_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        team.Join("observer", true);
        master.StartEstimation();
        master.Estimation = team.AvailableEstimations.Single(e => e.Value == 0);
        master.CancelEstimation();

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_DateTimeProvider_DateTimeProviderIsSet()
    {
        // Arrange
        var team = new ScrumTeam("test");
        team.SetScrumMaster("master");
        var dateTimeProvider = new DateTimeProviderMock();

        // Act
        var json = SerializeTeam(team);
        var result = DeserializeTeam(json, dateTimeProvider);

        // Verify
        Assert.AreEqual<DateTimeProvider>(dateTimeProvider, result.DateTimeProvider);
    }

    [TestMethod]
    public void SerializeAndDeserialize_MemberDisconnected_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        var observer = team.Join("observer", true);
        master.StartEstimation();
        team.Join("Bob", true);
        team.Join("Alice", false);
        member.Estimation = team.AvailableEstimations.Single(e => e.Value == 0.5);
        master.Estimation = team.AvailableEstimations.Single(e => e.Value == EstimationTestData.Infinity);
        team.Disconnect(observer.Name);
        team.Disconnect(member.Name);

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_AvailableEstimationsAreChanged_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        team.Join("observer", true);
        master.StartEstimation();
        master.CancelEstimation();
        team.ChangeAvailableEstimations(DeckProvider.Default.GetDeck(Deck.RockPaperScissorsLizardSpock));

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TimerStarted_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 01, DateTimeKind.Utc));
        team.SetScrumMaster("master");
        var member = (Member)team.Join("member", false);
        member.StartTimer(TimeSpan.FromMinutes(5));

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_TimerCanceled_CopyOfTheTeam()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 17, 8, 58, 01, DateTimeKind.Utc));
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        master.StartTimer(TimeSpan.FromMinutes(5));
        master.CancelTimer();

        // Act
        // Verify
        VerifySerialization(team);
    }

    [TestMethod]
    public void SerializeAndDeserialize_ReceivedMessages_MessageHasNextId()
    {
        // Arrange
        var team = new ScrumTeam("test");
        var master = team.SetScrumMaster("master");
        team.Join("member", false);
        team.Join("observer", true);
        master.StartEstimation();

        var lastMessage = master.Messages.Last();
        master.ClearMessages();

        // Act
        var result = VerifySerialization(team);

        // Verify
        var member = (Member?)result.FindMemberOrObserver("member");
        Assert.IsNotNull(member);
        member.Estimation = result.AvailableEstimations.First(e => e.Value == 5);

        Assert.IsNotNull(result.ScrumMaster);
        Assert.IsTrue(result.ScrumMaster.HasMessage);
        var message = result.ScrumMaster.Messages.First();
        Assert.AreEqual(lastMessage.Id + 1, message.Id);
    }

    private static ScrumTeam VerifySerialization(ScrumTeam scrumTeam)
    {
        var json = SerializeTeam(scrumTeam);
        var result = DeserializeTeam(json);
        ScrumTeamAsserts.AssertScrumTeamsAreEqual(scrumTeam, result);
        return result;
    }

    private static byte[] SerializeTeam(ScrumTeam scrumTeam, DateTimeProvider? dateTimeProvider = null)
    {
        using var stream = new MemoryStream();
        var serializer = new ScrumTeamSerializer(dateTimeProvider, GuidProvider.Default);
        serializer.Serialize(stream, scrumTeam);
        return stream.ToArray();
    }

    private static ScrumTeam DeserializeTeam(byte[] json, DateTimeProvider? dateTimeProvider = null)
    {
        using var stream = new MemoryStream(json);
        var serializer = new ScrumTeamSerializer(dateTimeProvider, GuidProvider.Default);
        return serializer.Deserialize(stream);
    }
}
