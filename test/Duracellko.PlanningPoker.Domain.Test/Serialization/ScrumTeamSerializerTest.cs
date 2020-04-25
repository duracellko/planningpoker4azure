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
            master.Estimation = team.AvailableEstimations.Single(e => e.Value.HasValue && double.IsPositiveInfinity(e.Value.Value));
            team.Disconnect(observer.Name);
            team.Disconnect(member.Name);

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

            var lastMessage = master.PopMessage();
            while (master.HasMessage)
            {
                lastMessage = master.PopMessage();
            }

            // Act
            var result = VerifySerialization(team);

            // Verify
            var member = (Member)result.FindMemberOrObserver("member");
            member.Estimation = result.AvailableEstimations.First(e => e.Value == 5);

            Assert.IsTrue(result.ScrumMaster.HasMessage);
            var message = result.ScrumMaster.PopMessage();
            Assert.AreEqual(lastMessage.Id + 1, message.Id);
        }

        private static ScrumTeam VerifySerialization(ScrumTeam scrumTeam)
        {
            var json = SerializeTeam(scrumTeam);
            var result = DeserializeTeam(json);
            ScrumTeamAsserts.AssertScrumTeamsAreEqual(scrumTeam, result);
            return result;
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
    }
}
