using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumTeamSerializationTest
    {
        [TestMethod]
        public void SerializeAndDeserialize_EmptyTeam_CopyOfTheTeam()
        {
            // Arrange
            var team = new ScrumTeam("test");

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<string>(team.Name, result.Name);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithScrumMaster_CopyOfTheTeam()
        {
            // Arrange
            var team = new ScrumTeam("test");
            team.SetScrumMaster("master");

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<string>(team.ScrumMaster.Name, result.ScrumMaster.Name);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithMember_CopyOfTheTeam()
        {
            // Arrange
            var team = new ScrumTeam("test");
            team.SetScrumMaster("master");
            var member = team.Join("member", false);

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<int>(2, result.Members.Count());
            var resultMember = result.Members.First(m => m != result.ScrumMaster);
            Assert.AreEqual<string>(member.Name, resultMember.Name);
            Assert.AreEqual<DateTime>(member.LastActivity, resultMember.LastActivity);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithObserver_CopyOfTheTeam()
        {
            // Arrange
            var team = new ScrumTeam("test");
            team.SetScrumMaster("master");
            var observer = team.Join("member", true);

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<int>(1, result.Observers.Count());
            var resultObserver = result.Observers.First();
            Assert.AreEqual<string>(observer.Name, resultObserver.Name);
            Assert.AreEqual<DateTime>(observer.LastActivity, resultObserver.LastActivity);
        }

        [TestMethod]
        public void SerializeAndDeserialize_EstimationStarted_CopyOfTheTeam()
        {
            // Arrange
            var team = new ScrumTeam("test");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);
            master.StartEstimation();

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<TeamState>(team.State, result.State);
            Assert.AreEqual<int>(master.Messages.Count(), result.ScrumMaster.Messages.Count());
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            var team = new ScrumTeam("test");
            int eventsCount = 0;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventsCount++);
            var master = team.SetScrumMaster("master");

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            eventsCount = 0;
            result.ScrumMaster.StartEstimation();
            Assert.AreEqual<int>(0, eventsCount);
        }

        [TestMethod]
        public void SerializeAndDeserialize_MemberMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            var team = new ScrumTeam("test");
            var master = team.SetScrumMaster("master");
            int eventsCount = 0;
            master.MessageReceived += new EventHandler((s, e) => eventsCount++);

            // Act
            var bytes = SerializeTeam(team);
            var result = DeserializeTeam(bytes);

            // Verify
            eventsCount = 0;
            result.ScrumMaster.StartEstimation();
            Assert.AreEqual<int>(0, eventsCount);
        }

        [TestMethod]
        public void SerializeAndDeserialize_DateTimeProviderAsContext_DateTimeProviderIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test");
            var dateTimeProvider = new DateTimeProviderMock();

            // Act
            var bytes = SerializeTeam(team);
            var streamingContext = new StreamingContext(StreamingContextStates.All, dateTimeProvider);
            var result = DeserializeTeam(bytes, streamingContext);

            // Verify
            Assert.AreEqual<DateTimeProvider>(dateTimeProvider, result.DateTimeProvider);
        }

        private static byte[] SerializeTeam(ScrumTeam team)
        {
            var formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, team);
                return memoryStream.ToArray();
            }
        }

        private static ScrumTeam DeserializeTeam(byte[] value)
        {
            return DeserializeTeam(value, null);
        }

        private static ScrumTeam DeserializeTeam(byte[] value, StreamingContext? context)
        {
            var formatter = context.HasValue ? new BinaryFormatter(null, context.Value) : new BinaryFormatter();
            using (var memoryStream = new MemoryStream(value))
            {
                return (ScrumTeam)formatter.Deserialize(memoryStream);
            }
        }
    }
}
