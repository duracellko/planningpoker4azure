using System.Collections.Generic;
using System.IO;
using System.Text;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Test.Service
{
    [TestClass]
    public class JsonSerializationTest
    {
        [TestMethod]
        public void JsonSerialize_ScrumTeam_Initial()
        {
            var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };

            var scrumTeam = new ScrumTeam
            {
                Name = "Test Team",
                State = TeamState.Initial,
                ScrumMaster = scrumMaster,
                Members = new List<TeamMember> { scrumMaster },
                AvailableEstimations = new List<Estimation>
                {
                    new Estimation { Value = null },
                    new Estimation { Value = 1 },
                    new Estimation { Value = 2 }
                }
            };

            var result = SerializeAndDeserialize(scrumTeam);

            Assert.IsNotNull(result);
            Assert.AreEqual(scrumTeam.Name, result.Name);
            Assert.AreEqual(scrumTeam.State, result.State);
            Assert.AreEqual(scrumTeam.ScrumMaster.Name, result.ScrumMaster.Name);
            Assert.AreEqual(scrumTeam.ScrumMaster.Type, result.ScrumMaster.Type);
            Assert.AreEqual(scrumTeam.Members[0].Name, result.Members[0].Name);
            Assert.AreEqual(scrumTeam.Members[0].Type, result.Members[0].Type);
            Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, result.AvailableEstimations[0].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, result.AvailableEstimations[1].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[2].Value, result.AvailableEstimations[2].Value);
        }

        [TestMethod]
        public void JsonSerialize_ScrumTeam_Estimated()
        {
            var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
            var teamMember = new TeamMember { Name = "John", Type = "Member" };

            var availableEstimations = new List<Estimation>
            {
                new Estimation { Value = 1 },
                new Estimation { Value = 2 }
            };

            var scrumTeam = new ScrumTeam
            {
                Name = "Test Team",
                State = TeamState.EstimationFinished,
                ScrumMaster = scrumMaster,
                Members = new List<TeamMember> { scrumMaster, teamMember },
                Observers = new List<TeamMember>
                {
                    new TeamMember { Name = "Jane", Type = "Observer" }
                },
                AvailableEstimations = availableEstimations,
                EstimationResult = new List<EstimationResultItem>
                {
                    new EstimationResultItem { Member = scrumMaster, Estimation = availableEstimations[1] },
                    new EstimationResultItem { Member = teamMember, Estimation = availableEstimations[0] }
                }
            };

            var result = SerializeAndDeserialize(scrumTeam);

            Assert.IsNotNull(result);
            Assert.AreEqual(scrumTeam.Name, result.Name);
            Assert.AreEqual(scrumTeam.State, result.State);
            Assert.AreEqual(scrumTeam.ScrumMaster.Name, result.ScrumMaster.Name);
            Assert.AreEqual(scrumTeam.ScrumMaster.Type, result.ScrumMaster.Type);
            Assert.AreEqual(scrumTeam.Members[0].Name, result.Members[0].Name);
            Assert.AreEqual(scrumTeam.Members[0].Type, result.Members[0].Type);
            Assert.AreEqual(scrumTeam.Members[1].Name, result.Members[1].Name);
            Assert.AreEqual(scrumTeam.Members[1].Type, result.Members[1].Type);
            Assert.AreEqual(scrumTeam.Observers[0].Name, result.Observers[0].Name);
            Assert.AreEqual(scrumTeam.Observers[0].Type, result.Observers[0].Type);
            Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, result.AvailableEstimations[0].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, result.AvailableEstimations[1].Value);
            Assert.AreEqual(scrumTeam.EstimationResult[0].Member.Name, result.EstimationResult[0].Member.Name);
            Assert.AreEqual(scrumTeam.EstimationResult[0].Member.Type, result.EstimationResult[0].Member.Type);
            Assert.AreEqual(scrumTeam.EstimationResult[0].Estimation.Value, result.EstimationResult[0].Estimation.Value);
            Assert.AreEqual(scrumTeam.EstimationResult[1].Member.Name, result.EstimationResult[1].Member.Name);
            Assert.AreEqual(scrumTeam.EstimationResult[1].Member.Type, result.EstimationResult[1].Member.Type);
            Assert.AreEqual(scrumTeam.EstimationResult[1].Estimation.Value, result.EstimationResult[1].Estimation.Value);
        }

        [TestMethod]
        public void JsonSerialize_ScrumTeam_EstimationInProgress()
        {
            var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
            var teamMember = new TeamMember { Name = "John", Type = "Member" };

            var availableEstimations = new List<Estimation>
            {
                new Estimation { Value = 1 },
                new Estimation { Value = 2 }
            };

            var scrumTeam = new ScrumTeam
            {
                Name = "Test Team",
                State = TeamState.EstimationInProgress,
                ScrumMaster = scrumMaster,
                Members = new List<TeamMember> { scrumMaster, teamMember },
                Observers = new List<TeamMember>(),
                AvailableEstimations = availableEstimations,
                EstimationParticipants = new List<EstimationParticipantStatus>
                {
                    new EstimationParticipantStatus { MemberName = teamMember.Name, Estimated = true },
                    new EstimationParticipantStatus { MemberName = scrumMaster.Name, Estimated = false }
                }
            };

            var result = SerializeAndDeserialize(scrumTeam);

            Assert.IsNotNull(result);
            Assert.AreEqual(scrumTeam.Name, result.Name);
            Assert.AreEqual(scrumTeam.State, result.State);
            Assert.AreEqual(scrumTeam.ScrumMaster.Name, result.ScrumMaster.Name);
            Assert.AreEqual(scrumTeam.ScrumMaster.Type, result.ScrumMaster.Type);
            Assert.AreEqual(scrumTeam.Members[0].Name, result.Members[0].Name);
            Assert.AreEqual(scrumTeam.Members[0].Type, result.Members[0].Type);
            Assert.AreEqual(scrumTeam.Members[1].Name, result.Members[1].Name);
            Assert.AreEqual(scrumTeam.Members[1].Type, result.Members[1].Type);
            Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, result.AvailableEstimations[0].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, result.AvailableEstimations[1].Value);
            Assert.AreEqual(scrumTeam.EstimationParticipants[0].MemberName, result.EstimationParticipants[0].MemberName);
            Assert.AreEqual(scrumTeam.EstimationParticipants[0].Estimated, result.EstimationParticipants[0].Estimated);
            Assert.AreEqual(scrumTeam.EstimationParticipants[1].MemberName, result.EstimationParticipants[1].MemberName);
            Assert.AreEqual(scrumTeam.EstimationParticipants[1].Estimated, result.EstimationParticipants[1].Estimated);
        }

        [TestMethod]
        public void JsonSerialize_ReconnectTeamResult_Initial()
        {
            var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };

            var scrumTeam = new ScrumTeam
            {
                Name = "Test Team",
                State = TeamState.Initial,
                ScrumMaster = scrumMaster,
                Members = new List<TeamMember> { scrumMaster },
                AvailableEstimations = new List<Estimation>
                {
                    new Estimation { Value = null },
                    new Estimation { Value = 1 },
                    new Estimation { Value = 2 }
                }
            };

            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 0
            };

            var result = SerializeAndDeserialize(reconnectTeamResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(reconnectTeamResult.LastMessageId, result.LastMessageId);
            Assert.IsNull(result.SelectedEstimation);

            var resultScrumTeam = result.ScrumTeam;
            Assert.AreEqual(scrumTeam.Name, resultScrumTeam.Name);
            Assert.AreEqual(scrumTeam.State, resultScrumTeam.State);
            Assert.AreEqual(scrumTeam.ScrumMaster.Name, resultScrumTeam.ScrumMaster.Name);
            Assert.AreEqual(scrumTeam.ScrumMaster.Type, resultScrumTeam.ScrumMaster.Type);
            Assert.AreEqual(scrumTeam.Members[0].Name, resultScrumTeam.Members[0].Name);
            Assert.AreEqual(scrumTeam.Members[0].Type, resultScrumTeam.Members[0].Type);
            Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, resultScrumTeam.AvailableEstimations[0].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, resultScrumTeam.AvailableEstimations[1].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[2].Value, resultScrumTeam.AvailableEstimations[2].Value);
        }

        [TestMethod]
        public void JsonSerialize_ReconnectTeamResult_EstimationInProgress()
        {
            var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
            var teamMember = new TeamMember { Name = "John", Type = "Member" };

            var availableEstimations = new List<Estimation>
            {
                new Estimation { Value = 1 },
                new Estimation { Value = 2 }
            };

            var scrumTeam = new ScrumTeam
            {
                Name = "Test Team",
                State = TeamState.EstimationInProgress,
                ScrumMaster = scrumMaster,
                Members = new List<TeamMember> { scrumMaster, teamMember },
                Observers = new List<TeamMember>(),
                AvailableEstimations = availableEstimations,
                EstimationParticipants = new List<EstimationParticipantStatus>
                {
                    new EstimationParticipantStatus { MemberName = teamMember.Name, Estimated = true },
                    new EstimationParticipantStatus { MemberName = scrumMaster.Name, Estimated = false }
                }
            };

            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 5147483647,
                SelectedEstimation = availableEstimations[1]
            };

            var result = SerializeAndDeserialize(reconnectTeamResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(reconnectTeamResult.LastMessageId, result.LastMessageId);
            Assert.AreEqual(reconnectTeamResult.SelectedEstimation.Value, result.SelectedEstimation.Value);

            var resultScrumTeam = result.ScrumTeam;
            Assert.AreEqual(scrumTeam.Name, resultScrumTeam.Name);
            Assert.AreEqual(scrumTeam.State, resultScrumTeam.State);
            Assert.AreEqual(scrumTeam.ScrumMaster.Name, resultScrumTeam.ScrumMaster.Name);
            Assert.AreEqual(scrumTeam.ScrumMaster.Type, resultScrumTeam.ScrumMaster.Type);
            Assert.AreEqual(scrumTeam.Members[0].Name, resultScrumTeam.Members[0].Name);
            Assert.AreEqual(scrumTeam.Members[0].Type, resultScrumTeam.Members[0].Type);
            Assert.AreEqual(scrumTeam.Members[1].Name, resultScrumTeam.Members[1].Name);
            Assert.AreEqual(scrumTeam.Members[1].Type, resultScrumTeam.Members[1].Type);
            Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, resultScrumTeam.AvailableEstimations[0].Value);
            Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, resultScrumTeam.AvailableEstimations[1].Value);
            Assert.AreEqual(scrumTeam.EstimationParticipants[0].MemberName, resultScrumTeam.EstimationParticipants[0].MemberName);
            Assert.AreEqual(scrumTeam.EstimationParticipants[0].Estimated, resultScrumTeam.EstimationParticipants[0].Estimated);
            Assert.AreEqual(scrumTeam.EstimationParticipants[1].MemberName, resultScrumTeam.EstimationParticipants[1].MemberName);
            Assert.AreEqual(scrumTeam.EstimationParticipants[1].Estimated, resultScrumTeam.EstimationParticipants[1].Estimated);
        }

        [TestMethod]
        public void JsonSerialize_Message_Empty()
        {
            var message = new Message
            {
                Id = 1,
                Type = MessageType.Empty
            };

            var result = SerializeAndDeserialize(message);

            Assert.IsNotNull(result);
            Assert.AreEqual(message.Id, result.Id);
            Assert.AreEqual(message.Type, result.Type);
        }

        [TestMethod]
        public void JsonSerialize_Message_EstimationStarted()
        {
            var message = new Message
            {
                Id = 2,
                Type = MessageType.EstimationStarted
            };

            var result = SerializeAndDeserialize(message);

            Assert.IsNotNull(result);
            Assert.AreEqual(message.Id, result.Id);
            Assert.AreEqual(message.Type, result.Type);
        }

        [TestMethod]
        public void JsonSerialize_Message_MemberJoined()
        {
            var message = new MemberMessage
            {
                Id = 2,
                Type = MessageType.MemberJoined,
                Member = new TeamMember { Name = "master", Type = "ScrumMaster" }
            };

            var result = SerializeAndDeserialize<Message>(message);

            Assert.IsNotNull(result);
            Assert.AreEqual(message.Id, result.Id);
            Assert.AreEqual(message.Type, result.Type);

            Assert.IsInstanceOfType(result, typeof(MemberMessage));
            var memberMessageResult = (MemberMessage)result;
            Assert.AreEqual(message.Member.Name, memberMessageResult.Member.Name);
            Assert.AreEqual(message.Member.Type, memberMessageResult.Member.Type);
        }

        [TestMethod]
        public void JsonSerialize_Message_EstimationEnded()
        {
            var message = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
                {
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = "master", Type = "ScrumMaster" },
                        Estimation = new Estimation { Value = 8 }
                    },
                    new EstimationResultItem
                    {
                        Member = new TeamMember { Name = "dev", Type = "Member" },
                        Estimation = new Estimation { Value = 13 }
                    }
                }
            };

            var result = SerializeAndDeserialize<Message>(message);

            Assert.IsNotNull(result);
            Assert.AreEqual(message.Id, result.Id);
            Assert.AreEqual(message.Type, result.Type);

            Assert.IsInstanceOfType(result, typeof(EstimationResultMessage));
            var estimationResult = (EstimationResultMessage)result;
            Assert.AreEqual(message.EstimationResult[0].Member.Name, estimationResult.EstimationResult[0].Member.Name);
            Assert.AreEqual(message.EstimationResult[0].Member.Type, estimationResult.EstimationResult[0].Member.Type);
            Assert.AreEqual(message.EstimationResult[0].Estimation.Value, estimationResult.EstimationResult[0].Estimation.Value);
            Assert.AreEqual(message.EstimationResult[1].Member.Name, estimationResult.EstimationResult[1].Member.Name);
            Assert.AreEqual(message.EstimationResult[1].Member.Type, estimationResult.EstimationResult[1].Member.Type);
            Assert.AreEqual(message.EstimationResult[1].Estimation.Value, estimationResult.EstimationResult[1].Estimation.Value);
        }

        private static T SerializeAndDeserialize<T>(T value)
        {
            var serialier = JsonSerializer.CreateDefault();
            var json = new StringBuilder();

            using (var writer = new StringWriter(json))
            {
                serialier.Serialize(writer, value, typeof(T));
            }

            using (var reader = new StringReader(json.ToString()))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return serialier.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}
