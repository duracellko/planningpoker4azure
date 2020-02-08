using System;
using System.Collections.Generic;
using System.Linq;
using Duracellko.PlanningPoker.Domain.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test.Serialization
{
    [TestClass]
    public class ScrumTeamDeserializationTest
    {
        [TestMethod]
        public void ScrumTeam_NoMembers_CreatesScrumTeam()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.EstimationCanceled,
            };

            // Act
            var result = new ScrumTeam(scrumTeamData, DateTimeProvider.Default);

            // Assert
            Assert.AreEqual(scrumTeamData.Name, result.Name);
            Assert.AreEqual(scrumTeamData.State, result.State);
        }

        [TestMethod]
        public void ScrumTeam_3Members_CreatesScrumTeam()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.Member, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                    new MemberData { MemberType = MemberType.Observer, Name = "PO" }
                }
            };

            // Act
            var result = new ScrumTeam(scrumTeamData, DateTimeProvider.Default);

            // Assert
            Assert.AreEqual(scrumTeamData.Name, result.Name);
            Assert.AreEqual(scrumTeamData.State, result.State);
            Assert.AreEqual(scrumTeamData.Members[1].Name, result.ScrumMaster.Name);
            var member = result.Members.First(m => m.GetType() == typeof(Member));
            Assert.AreEqual(scrumTeamData.Members[0].Name, member.Name);
            var observer = result.Observers.First();
            Assert.AreEqual(scrumTeamData.Members[2].Name, observer.Name);
        }

        [TestMethod]
        public void ScrumTeam_EmptyTeamName_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = string.Empty,
                State = TeamState.EstimationCanceled,
            };

            // Act
            // Assert
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_TeamNameIsNull_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = string.Empty,
                State = TeamState.EstimationCanceled,
            };

            // Act
            // Assert
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [DataTestMethod]
        [DataRow(MemberType.ScrumMaster, null)]
        [DataRow(MemberType.ScrumMaster, "")]
        [DataRow(MemberType.Member, null)]
        [DataRow(MemberType.Member, "")]
        [DataRow(MemberType.Observer, null)]
        [DataRow(MemberType.Observer, "")]
        public void ScrumTeam_EmptyMemberName_ArgumentException(MemberType emptyMemberType, string name)
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.Member, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                    new MemberData { MemberType = MemberType.Observer, Name = "PO" }
                }
            };

            var emptyMember = scrumTeamData.Members.First(m => m.MemberType == emptyMemberType);
            emptyMember.Name = name;

            // Act
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_ScrumMasterAndMemberHaveSameNames_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.Member, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "the member" },
                    new MemberData { MemberType = MemberType.Observer, Name = "PO" }
                }
            };

            // Act
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_ScrumMasterAndObserverHaveSameNames_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.Member, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                    new MemberData { MemberType = MemberType.Observer, Name = "Master" }
                }
            };

            // Act
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_MemberAndObserverHaveSameNames_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.Member, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                    new MemberData { MemberType = MemberType.Observer, Name = "the member" }
                }
            };

            // Act
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_2ScrumMasters_InvalidOperationException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "the member" },
                    new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" }
                }
            };

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }

        [TestMethod]
        public void ScrumTeam_InvalidMessage_ArgumentException()
        {
            // Arrange
            var scrumTeamData = new ScrumTeamData
            {
                Name = "The Team",
                State = TeamState.Initial,
                Members = new List<MemberData>
                {
                    new MemberData
                    {
                        MemberType = MemberType.ScrumMaster,
                        Name = "Master",
                        LastMessageId = 1,
                        Messages = new List<MessageData>
                        {
                            new MessageData { Id = 1, MessageType = MessageType.MemberActivity }
                        }
                    }
                }
            };

            // Act
            Assert.ThrowsException<ArgumentException>(() => new ScrumTeam(scrumTeamData, DateTimeProvider.Default));
        }
    }
}
