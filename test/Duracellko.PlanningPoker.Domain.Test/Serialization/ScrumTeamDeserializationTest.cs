using System;
using System.Collections.Generic;
using System.Linq;
using Duracellko.PlanningPoker.Domain.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test.Serialization;

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
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.EstimationCanceled,
        };

        // Act
        var result = CreateScrumTeam(scrumTeamData);

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
            AvailableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci).ToList(),
            State = TeamState.Initial,
            TimerEndTime = new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Unspecified),
            Members = new List<MemberData>
            {
                new MemberData { MemberType = MemberType.Member, Name = "the member" },
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                new MemberData { MemberType = MemberType.Observer, Name = "PO" }
            }
        };

        // Act
        var result = CreateScrumTeam(scrumTeamData);

        // Assert
        Assert.AreEqual(scrumTeamData.Name, result.Name);
        Assert.AreEqual(scrumTeamData.State, result.State);
        Assert.AreEqual(new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc), result.TimerEndTime);
        Assert.AreEqual(DateTimeKind.Utc, result.TimerEndTime!.Value.Kind);
        Assert.IsNotNull(result.ScrumMaster);
        Assert.AreEqual(scrumTeamData.Members[1].Name, result.ScrumMaster.Name);
        var member = result.Members.First(m => m.GetType() == typeof(Member));
        Assert.AreEqual(scrumTeamData.Members[0].Name, member.Name);
        var observer = result.Observers.First();
        Assert.AreEqual(scrumTeamData.Members[2].Name, observer.Name);

        var expectedCollection = new double?[]
        {
            0.0, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 21.0, 34.0, 55.0, 89.0, double.PositiveInfinity, null
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.AvailableEstimations.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void ScrumTeam_AvailableEstimationsIsNull_ArgumentNullException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            State = TeamState.EstimationCanceled,
            AvailableEstimations = null!
        };

        // Act
        // Assert
        Assert.ThrowsException<ArgumentNullException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_EmptyTeamName_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = string.Empty,
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.EstimationCanceled,
        };

        // Act
        // Assert
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_TeamNameIsNull_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = null!,
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.EstimationCanceled,
        };

        // Act
        // Assert
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
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
            AvailableEstimations = GetAvailableEstimations(),
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
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_ScrumMasterAndMemberHaveSameNames_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.Initial,
            Members = new List<MemberData>
            {
                new MemberData { MemberType = MemberType.Member, Name = "the member" },
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "the member" },
                new MemberData { MemberType = MemberType.Observer, Name = "PO" }
            }
        };

        // Act
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_ScrumMasterAndObserverHaveSameNames_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.Initial,
            Members = new List<MemberData>
            {
                new MemberData { MemberType = MemberType.Member, Name = "the member" },
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                new MemberData { MemberType = MemberType.Observer, Name = "Master" }
            }
        };

        // Act
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_MemberAndObserverHaveSameNames_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.Initial,
            Members = new List<MemberData>
            {
                new MemberData { MemberType = MemberType.Member, Name = "the member" },
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" },
                new MemberData { MemberType = MemberType.Observer, Name = "the member" }
            }
        };

        // Act
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_2ScrumMasters_InvalidOperationException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            AvailableEstimations = GetAvailableEstimations(),
            State = TeamState.Initial,
            Members = new List<MemberData>
            {
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "the member" },
                new MemberData { MemberType = MemberType.ScrumMaster, Name = "Master" }
            }
        };

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => CreateScrumTeam(scrumTeamData));
    }

    [TestMethod]
    public void ScrumTeam_InvalidMessage_ArgumentException()
    {
        // Arrange
        var scrumTeamData = new ScrumTeamData
        {
            Name = "The Team",
            AvailableEstimations = GetAvailableEstimations(),
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
        Assert.ThrowsException<ArgumentException>(() => CreateScrumTeam(scrumTeamData));
    }

    private static List<Estimation> GetAvailableEstimations()
    {
        return DeckProvider.Default.GetDefaultDeck().ToList();
    }

    private static ScrumTeam CreateScrumTeam(ScrumTeamData scrumTeamData)
    {
        return new ScrumTeam(scrumTeamData, DateTimeProvider.Default, GuidProvider.Default);
    }
}
