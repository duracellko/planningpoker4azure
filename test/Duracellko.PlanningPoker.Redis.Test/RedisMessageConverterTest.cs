using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace Duracellko.PlanningPoker.Redis.Test
{
    [TestClass]
    public class RedisMessageConverterTest
    {
        private const string SenderId = "3d1c7636-ae1d-4288-b1e1-0dccc8989722";
        private const string RecipientId = "10243241-802e-4d66-b4fc-55c76c23bcb2";
        private const string TeamName = "My Team";
        private const string Team1Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":3,\"LastActivity\":\"2020-05-24T14:46:48.1509407Z\",\"IsDormant\":false,\"Estimation\":null},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":2,\"LastActivity\":\"2020-05-24T14:47:40.119354Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":20.0}}],\"EstimationResult\":{\"Duracellko\":null,\"Me\":{\"Value\":20.0}}}";
        private const string Team2Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":9,\"LastActivity\":\"2020-05-24T14:53:07.6381166Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":2.0}},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":8,\"LastActivity\":\"2020-05-24T14:53:05.8193334Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":5.0}},{\"Name\":\"Test\",\"MemberType\":1,\"Messages\":[{\"Id\":4,\"MessageType\":6,\"MemberName\":\"Duracellko\",\"EstimationResult\":null},{\"Id\":5,\"MessageType\":6,\"MemberName\":\"Me\",\"EstimationResult\":null}],\"LastMessageId\":5,\"LastActivity\":\"2020-05-24T14:52:40.0708949Z\",\"IsDormant\":false,\"Estimation\":null}],\"EstimationResult\":{\"Duracellko\":{\"Value\":2.0},\"Me\":{\"Value\":5.0},\"Test\":null}}";

        [TestMethod]
        public void ConvertToRedisMessage_Null_ArgumentNullException()
        {
            var target = new RedisMessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.ConvertToRedisMessage(null!));
        }

        [TestMethod]
        public void ConvertToNodeMessage_Null_ArgumentNullException()
        {
            var target = new RedisMessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.ConvertToNodeMessage(RedisValue.Null));
        }

        [TestMethod]
        public void ConvertToNodeMessage_EmptyString_ArgumentNullException()
        {
            var target = new RedisMessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.ConvertToNodeMessage(RedisValue.EmptyString));
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_ScrumTeamMessage()
        {
            var scrumTeamMessage = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMessage)result.Data!;

            Assert.AreEqual(MessageType.EstimationStarted, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_ScrumTeamMemberMessage()
        {
            var scrumTeamMessage = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberType = "Observer",
                MemberName = "Test person",
                SessionId = Guid.NewGuid()
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberMessage)result.Data!;

            Assert.AreEqual(MessageType.MemberJoined, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberType, resultData.MemberType);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
            Assert.AreEqual(scrumTeamMessage.SessionId, resultData.SessionId);
        }

        [DataTestMethod]
        [DataRow(8.0)]
        [DataRow(0.5)]
        [DataRow(0.0)]
        [DataRow(null)]
        [DataRow(double.PositiveInfinity)]
        public void ConvertToRedisMessageAndBack_ScrumTeamMemberEstimationMessage(double? estimation)
        {
            var scrumTeamMessage = new ScrumTeamMemberEstimationMessage(TeamName, MessageType.MemberEstimated)
            {
                MemberName = "Scrum Master",
                Estimation = estimation
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberEstimationMessage)result.Data!;

            Assert.AreEqual(MessageType.MemberEstimated, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
            Assert.AreEqual(scrumTeamMessage.Estimation, resultData.Estimation);
        }

        [TestMethod]
        public void ConvertToRedisBusMessageAndBack_ScrumTeamEstimationSetMessage()
        {
            var deck = DeckProvider.Default.GetDefaultDeck().Select(e => e.Value).ToList();
            var scrumTeamMessage = new ScrumTeamEstimationSetMessage(TeamName, MessageType.AvailableEstimationsChanged)
            {
                Estimations = deck
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamEstimationSetMessage)result.Data!;

            Assert.AreEqual(MessageType.AvailableEstimationsChanged, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            CollectionAssert.AreEqual(deck, resultData.Estimations.ToList());
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_ScrumTeamTimerMessage()
        {
            var scrumTeamMessage = new ScrumTeamTimerMessage(TeamName, MessageType.TimerStarted)
            {
                EndTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc)
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamTimerMessage)result.Data!;

            Assert.AreEqual(MessageType.TimerStarted, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.EndTime, resultData.EndTime);
            Assert.AreEqual(DateTimeKind.Utc, resultData.EndTime.Kind);
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_TeamCreated()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.TeamCreated)
            {
                SenderNodeId = SenderId,
                Data = Encoding.UTF8.GetBytes(Team1Json)
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);

            var resultJson = Encoding.UTF8.GetString((byte[])result.Data!);
            Assert.AreEqual(Team1Json, resultJson);
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_RequestTeamList()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList)
            {
                SenderNodeId = SenderId
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
        public void ConvertToRedisMessageAndBack_TeamList()
        {
            var teamList = new[] { TeamName, "Test", "Hello, World!" };
            var nodeMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[]?)result.Data);
        }

        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable array")]
        public void ConvertToRedisMessageAndBack_RequestTeams()
        {
            var teamList = new[] { TeamName };
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[]?)result.Data);
        }

        [TestMethod]
        public void ConvertToRedisMessageAndBack_InitializeTeam()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = Encoding.UTF8.GetBytes(Team2Json)
            };

            var result = ConvertToRedisMessageAndBack(nodeMessage);

            var resultJson = Encoding.UTF8.GetString((byte[])result.Data!);
            Assert.AreEqual(Team2Json, resultJson);
        }

        [TestMethod]
        public void GetMessageHeader_Null_ArgumentNullException()
        {
            var target = new RedisMessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.GetMessageHeader(RedisValue.Null));
        }

        [TestMethod]
        public void GetMessageHeader_EmptyString_ArgumentNullException()
        {
            var target = new RedisMessageConverter();
            Assert.ThrowsException<ArgumentNullException>(() => target.GetMessageHeader(RedisValue.EmptyString));
        }

        [TestMethod]
        public void GetMessageHeader_ScrumTeamMessage_ReturnsMessageTypeAndSenderAndRecipient()
        {
            var scrumTeamMessage = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var target = new RedisMessageConverter();
            var redisMessage = target.ConvertToRedisMessage(nodeMessage);

            var result = target.GetMessageHeader(redisMessage);

            Assert.IsNotNull(result);
            Assert.AreNotSame(nodeMessage, result);
            Assert.AreEqual(nodeMessage.MessageType, result.MessageType);
            Assert.AreEqual(nodeMessage.SenderNodeId, result.SenderNodeId);
            Assert.AreEqual(nodeMessage.RecipientNodeId, result.RecipientNodeId);
            Assert.IsNull(result.Data);
        }

        private static NodeMessage ConvertToRedisMessageAndBack(NodeMessage nodeMessage)
        {
            var target = new RedisMessageConverter();
            var redisMessage = target.ConvertToRedisMessage(nodeMessage);

            Assert.AreNotEqual(RedisValue.Null, redisMessage);
            Assert.AreNotEqual(RedisValue.EmptyString, redisMessage);

            var result = target.ConvertToNodeMessage(redisMessage);

            Assert.IsNotNull(result);
            Assert.AreNotSame(nodeMessage, result);
            Assert.AreEqual(nodeMessage.MessageType, result.MessageType);
            Assert.AreEqual(nodeMessage.SenderNodeId, result.SenderNodeId);
            Assert.AreEqual(nodeMessage.RecipientNodeId, result.RecipientNodeId);

            if (nodeMessage.Data == null)
            {
                Assert.IsNull(result.Data);
            }
            else
            {
                Assert.IsNotNull(result.Data);
                Assert.AreEqual(nodeMessage.Data.GetType(), result.Data.GetType());
            }

            return result;
        }
    }
}
