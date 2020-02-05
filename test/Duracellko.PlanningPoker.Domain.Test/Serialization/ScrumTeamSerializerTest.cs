using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Domain.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test.Serialization
{
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
            master.Estimation = team.AvailableEstimations.Single(e => !e.Value.HasValue);

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
            master.Estimation = team.AvailableEstimations.Single(e => e.Value.HasValue && double.IsPositiveInfinity(e.Value.Value));

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
        public void SerializeAndDeserialize_TeamMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            var team = new ScrumTeam("test");
            var master = team.SetScrumMaster("master");

            // Act
            var json = SerializeTeam(team);
            var result = DeserializeTeam(json);

            // Verify
            int eventsCount = 0;
            result.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventsCount++);
            result.ScrumMaster.StartEstimation();
            Assert.AreEqual<int>(0, eventsCount);
        }

        [TestMethod]
        public void SerializeAndDeserialize_MemberMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            var team = new ScrumTeam("test");
            team.SetScrumMaster("master");

            // Act
            var json = SerializeTeam(team);
            var result = DeserializeTeam(json);

            // Verify
            var master = result.ScrumMaster;
            int eventsCount = 0;
            master.MessageReceived += new EventHandler((s, e) => eventsCount++);
            master.StartEstimation();
            Assert.AreEqual<int>(0, eventsCount);
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

        private static void VerifySerialization(ScrumTeam scrumTeam)
        {
            var json = SerializeTeam(scrumTeam);
            var result = DeserializeTeam(json);
            AssertScrumTeamsAreEqual(scrumTeam, result);
        }

        private static string SerializeTeam(ScrumTeam scrumTeam, DateTimeProvider dateTimeProvider = null)
        {
            var result = new StringBuilder();
            using (var writer = new StringWriter(result))
            {
                var serializer = new ScrumTeamSerializer(dateTimeProvider);
                serializer.Serialize(writer, scrumTeam);
            }

            return result.ToString();
        }

        private static ScrumTeam DeserializeTeam(string json, DateTimeProvider dateTimeProvider = null)
        {
            using (var reader = new StringReader(json))
            {
                var serializer = new ScrumTeamSerializer(dateTimeProvider);
                return serializer.Deserialize(reader);
            }
        }

        private static void AssertScrumTeamsAreEqual(ScrumTeam expected, ScrumTeam actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.State, actual.State);
            AssertAvailableEstimations(expected.AvailableEstimations, actual.AvailableEstimations);
            AssertObserversAreEqual(expected.ScrumMaster, actual.ScrumMaster);

            Assert.AreEqual(expected.Members.Count(), actual.Members.Count());
            foreach (var expectedMember in expected.Members)
            {
                var actualMember = actual.Members.Single(m => m.Name == expectedMember.Name);
                AssertObserversAreEqual(expectedMember, actualMember);
            }

            Assert.AreEqual(expected.Observers.Count(), actual.Observers.Count());
            foreach (var expectedObserver in expected.Observers)
            {
                var actualObserver = actual.Observers.Single(m => m.Name == expectedObserver.Name);
                AssertObserversAreEqual(expectedObserver, actualObserver);
            }

            AssertEsimationParticipantsAreEqual(expected.EstimationParticipants, actual.EstimationParticipants);
            AssertEstimationResultsAreEqual(expected.EstimationResult, actual.EstimationResult);
        }

        private static void AssertAvailableEstimations(IEnumerable<Estimation> expected, IEnumerable<Estimation> actual)
        {
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        private static void AssertObserversAreEqual(Observer expected, Observer actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.GetType(), actual.GetType());
                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.LastActivity, actual.LastActivity);

                Assert.AreEqual(expected.HasMessage, actual.HasMessage);
                var expectedMessages = expected.Messages.ToList();
                var actualMessages = actual.Messages.ToList();
                Assert.AreEqual(expectedMessages.Count, actualMessages.Count);

                for (int i = 0; i < expectedMessages.Count; i++)
                {
                    AssertMessagesAreEqual(expectedMessages[i], actualMessages[i]);
                }

                if (expected is Member expectedMember)
                {
                    AssertMembersAreEqual(expectedMember, (Member)actual);
                }
            }
        }

        private static void AssertMembersAreEqual(Member expected, Member actual)
        {
            Assert.AreEqual(expected.Estimation, actual.Estimation);
        }

        private static void AssertMessagesAreEqual(Message expected, Message actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.MessageType, actual.MessageType);
            Assert.AreEqual(expected.GetType(), actual.GetType());

            if (expected is MemberMessage expectedMemberMessage)
            {
                AssertMemberMessagesAreEqual(expectedMemberMessage, (MemberMessage)actual);
            }

            if (expected is EstimationResultMessage expectedEstimationResultMessage)
            {
                AssertEstimationResultMessagesAreEqual(expectedEstimationResultMessage, (EstimationResultMessage)actual);
            }
        }

        private static void AssertMemberMessagesAreEqual(MemberMessage expected, MemberMessage actual)
        {
            AssertObserversAreEqual(expected.Member, actual.Member);
        }

        private static void AssertEstimationResultMessagesAreEqual(EstimationResultMessage expected, EstimationResultMessage actual)
        {
            AssertEstimationResultsAreEqual(expected.EstimationResult, actual.EstimationResult);
        }

        private static void AssertEsimationParticipantsAreEqual(IEnumerable<EstimationParticipantStatus> expected, IEnumerable<EstimationParticipantStatus> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count());
            foreach (var expectedItem in expected)
            {
                var actualItem = actual.Single(i => i.MemberName == expectedItem.MemberName);
                Assert.AreEqual(expectedItem.MemberName, actualItem.MemberName);
                Assert.AreEqual(expectedItem.Estimated, actualItem.Estimated);
            }
        }

        private static void AssertEstimationResultsAreEqual(EstimationResult expected, EstimationResult actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var expectedItem in expected)
            {
                var actualItem = actual.Single(i => i.Key.Name == expectedItem.Key.Name);
                AssertObserversAreEqual(expectedItem.Key, actualItem.Key);
                Assert.AreEqual(expectedItem.Value, actualItem.Value);
            }
        }
    }
}
