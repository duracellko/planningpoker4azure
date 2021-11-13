using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ObserverTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var name = "test";

            // Act
            var result = new Observer(team, name);

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
            var result = new Observer(team, name);

            // Verify
            Assert.AreEqual<Guid>(Guid.Empty, result.SessionId);
            Assert.AreEqual<long>(0, result.AcknowledgedMessageId);
        }

        [TestMethod]
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            var name = "test";

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new Observer(null!, name));
        }

        [TestMethod]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var team = new ScrumTeam("test team");

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new Observer(team, string.Empty));
        }

        [TestMethod]
        public void HasMessages_GetAfterConstruction_ReturnsFalse()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.HasMessage;

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Messages_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.Messages;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void AcknowledgeMessages_SessionIdIsValid_MessageQueueIsNotChanged(int lastMessageId)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");
            target.SessionId = sessionId;

            // Act
            target.AcknowledgeMessages(sessionId, lastMessageId);

            // Verify
            Assert.IsFalse(target.HasMessage);
            Assert.IsFalse(target.Messages.Any());
            Assert.AreEqual(lastMessageId, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void AcknowledgeMessages_SessionIdIsNotValid_ArgumentException(int lastMessageId)
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");
            target.SessionId = Guid.NewGuid();

            // Act
            var exception = Assert.ThrowsException<ArgumentException>(() => target.AcknowledgeMessages(Guid.NewGuid(), lastMessageId));

            // Verify
            Assert.AreEqual("sessionId", exception.ParamName);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void AcknowledgeMessages_SessionIdIsZeroGuid_ArgumentException(int lastMessageId)
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var exception = Assert.ThrowsException<ArgumentException>(() => target.AcknowledgeMessages(Guid.Empty, lastMessageId));

            // Verify
            Assert.AreEqual("sessionId", exception.ParamName);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(int.MinValue)]
        public void AcknowledgeMessages_LastMessageIdIsZeroOrNegative_MessageQueueIsNotChanged(int lastMessageId)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = CreateScrumTeamWithMessages(sessionId);
            var target = team.Observers.First();
            var expectedMessages = target.Messages.ToList();

            // Act
            target.AcknowledgeMessages(sessionId, lastMessageId);

            // Verify
            Assert.AreEqual(5, target.Messages.Count());
            CollectionAssert.AreEqual(expectedMessages, target.Messages.ToList());
            Assert.AreEqual(0, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void AcknowledgeMessages_LastMessageIdIsLessThanMessageCount_MessagesAreRemoved(int lastMessageId)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = CreateScrumTeamWithMessages(sessionId);
            var target = team.Observers.First();
            var expectedMessages = target.Messages.Skip(lastMessageId).ToList();

            // Act
            target.AcknowledgeMessages(sessionId, lastMessageId);

            // Verify
            Assert.AreEqual(5 - lastMessageId, target.Messages.Count());
            CollectionAssert.AreEqual(expectedMessages, target.Messages.ToList());
            Assert.AreEqual(lastMessageId, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(10)]
        [DataRow(int.MaxValue)]
        public void AcknowledgeMessages_LastMessageIdIsMoreThanMessageCount_AllMessagesAreRemoved(int lastMessageId)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = CreateScrumTeamWithMessages(sessionId);
            var target = team.Observers.First();

            // Act
            target.AcknowledgeMessages(sessionId, lastMessageId);

            // Verify
            Assert.IsFalse(target.HasMessage);
            Assert.IsFalse(target.Messages.Any());
            Assert.AreEqual(lastMessageId, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(1, 0)]
        [DataRow(2, 2)]
        [DataRow(3, 1)]
        [DataRow(4, 3)]
        public void AcknowledgeMessages_AcknowledgeSecondTimeWithLowerMessageId_AcknowledgedMessageIdFromFirstCall(int messageId1, int messageId2)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = CreateScrumTeamWithMessages(sessionId);
            var target = team.Observers.First();
            var expectedMessages = target.Messages.Skip(messageId1).ToList();

            // Act
            target.AcknowledgeMessages(sessionId, messageId1);
            target.AcknowledgeMessages(sessionId, messageId2);

            // Verify
            Assert.AreEqual(5 - messageId1, target.Messages.Count());
            CollectionAssert.AreEqual(expectedMessages, target.Messages.ToList());
            Assert.AreEqual(messageId1, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(5, 2)]
        [DataRow(6, -5)]
        [DataRow(10, 0)]
        [DataRow(int.MaxValue, 5)]
        public void AcknowledgeMessages_AcknowledgeSecondTimeWithOlderMessageId_AcknowledgedMessageIdFromFirstCall(int messageId1, int messageId2)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = CreateScrumTeamWithMessages(sessionId);
            var target = team.Observers.First();
            var expectedMessages = target.Messages.Skip(messageId1).ToList();

            // Act
            target.AcknowledgeMessages(sessionId, messageId1);
            target.AcknowledgeMessages(sessionId, messageId2);

            // Verify
            Assert.IsFalse(target.HasMessage);
            Assert.IsFalse(target.Messages.Any());
            Assert.AreEqual(messageId1, target.AcknowledgedMessageId);
        }

        [TestMethod]
        public void ClearMessages_AfterConstruction_ReturnsZero()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(0, result);
            Assert.IsFalse(target.HasMessage);
            Assert.AreEqual<long>(result, target.AcknowledgedMessageId);
        }

        [TestMethod]
        public void ClearMessages_After2Messages_HasNoMessages()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var target = team.Join("test", true);
            master.StartEstimation();
            master.CancelEstimation();

            // Act
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(2, result);
            Assert.IsFalse(target.HasMessage);
            Assert.AreEqual<long>(result, target.AcknowledgedMessageId);
        }

        [TestMethod]
        public void ClearMessages_AfterAcknowledge_HasNoMessages()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: new GuidProviderMock(sessionId));
            var master = team.SetScrumMaster("master");
            var target = team.Join("test", true);
            master.StartEstimation();
            master.CancelEstimation();

            // Act
            target.AcknowledgeMessages(sessionId, 2);
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(2, result);
            Assert.IsFalse(target.HasMessage);
            Assert.AreEqual<long>(result, target.AcknowledgedMessageId);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(6)]
        public void ClearMessages_After5Messages_HasNoMessages(int acknowledgeMessageId)
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var team = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: new GuidProviderMock(sessionId));
            var master = team.SetScrumMaster("master");
            var target = team.Join("test", true);
            master.StartEstimation();
            master.CancelEstimation();
            target.AcknowledgeMessages(sessionId, acknowledgeMessageId);
            master.StartEstimation();
            master.Estimation = team.AvailableEstimations.First(e => e.Value == 1);

            // Act
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(5, result);
            Assert.IsFalse(target.HasMessage);
            Assert.AreEqual<long>(result, target.AcknowledgedMessageId);
        }

        [TestMethod]
        public void LastActivity_AfterConstruction_ReturnsUtcNow()
        {
            // Arrange
            var utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(utcNow);

            var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
            var target = new Observer(team, "test");

            // Act
            var result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }

        [TestMethod]
        public void UpdateActivity_UtcNowIsChanged_LastActivityIsChanged()
        {
            // Arrange
            var utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 2, 3, 35, 0));

            var team = ScrumTeamTestData.CreateScrumTeam("test team", dateTimeProvider: dateTimeProvider);
            var target = new Observer(team, "test");
            dateTimeProvider.SetUtcNow(utcNow);

            // Act
            target.UpdateActivity();
            var result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }

        private static ScrumTeam CreateScrumTeamWithMessages(Guid sessionId)
        {
            var team = ScrumTeamTestData.CreateScrumTeam("test team", guidProvider: new GuidProviderMock(sessionId));
            team.SetScrumMaster("test master");
            team.Join("test", true);
            var member = (Member)team.Join("test member", false);

            team.ScrumMaster!.StartEstimation();
            member.Estimation = team.AvailableEstimations.First(e => e.Value == 8);
            team.ScrumMaster.Estimation = team.AvailableEstimations.First(e => e.Value == 3);

            return team;
        }
    }
}
