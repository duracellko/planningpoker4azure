using System;
using System.Collections.Generic;
using System.Text.Json;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Test.Service;

[TestClass]
public class JsonSerializationTest
{
    public static IEnumerable<object[]> JsonTestData { get; } = new[]
    {
        new object[] { JsonSerializerDefaults.General },
        new object[] { JsonSerializerDefaults.Web },
    };

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_ScrumTeam_Initial(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };

        var scrumTeam = new ScrumTeam
        {
            Name = "Test Team",
            State = TeamState.Initial,
            ScrumMaster = scrumMaster,
            Members = [scrumMaster],
            AvailableEstimations =
            [
                new Estimation { Value = null },
                new Estimation { Value = 1 },
                new Estimation { Value = 2 }
            ]
        };

        var result = SerializeAndDeserialize(scrumTeam, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(scrumTeam.Name, result.Name);
        Assert.AreEqual(scrumTeam.State, result.State);
        Assert.AreEqual(scrumTeam.TimerEndTime, result.TimerEndTime);
        Assert.IsNotNull(result.ScrumMaster);
        Assert.AreEqual(scrumTeam.ScrumMaster.Name, result.ScrumMaster.Name);
        Assert.AreEqual(scrumTeam.ScrumMaster.Type, result.ScrumMaster.Type);
        Assert.AreEqual(scrumTeam.Members[0].Name, result.Members[0].Name);
        Assert.AreEqual(scrumTeam.Members[0].Type, result.Members[0].Type);
        Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, result.AvailableEstimations[0].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, result.AvailableEstimations[1].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[2].Value, result.AvailableEstimations[2].Value);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_ScrumTeam_Estimated(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
        var teamMember = new TeamMember { Name = "John", Type = "Member" };

        var availableEstimations = new List<Estimation>
        {
            new() { Value = 1 },
            new() { Value = 2 }
        };

        var scrumTeam = new ScrumTeam
        {
            Name = "Test Team",
            State = TeamState.EstimationFinished,
            ScrumMaster = scrumMaster,
            TimerEndTime = new DateTime(2021, 11, 17, 8, 58, 1, DateTimeKind.Utc),
            Members = [scrumMaster, teamMember],
            Observers =
            [
                new TeamMember { Name = "Jane", Type = "Observer" }
            ],
            AvailableEstimations = availableEstimations,
            EstimationResult =
            [
                new EstimationResultItem { Member = scrumMaster, Estimation = availableEstimations[1] },
                new EstimationResultItem { Member = teamMember, Estimation = availableEstimations[0] }
            ]
        };

        var result = SerializeAndDeserialize(scrumTeam, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(scrumTeam.Name, result.Name);
        Assert.AreEqual(scrumTeam.State, result.State);
        Assert.AreEqual(scrumTeam.TimerEndTime, result.TimerEndTime);
        Assert.IsNotNull(result.ScrumMaster);
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
        Assert.IsNotNull(result.EstimationResult);
        Assert.AreEqual(scrumTeam.EstimationResult[0].Member!.Name, result.EstimationResult[0].Member!.Name);
        Assert.AreEqual(scrumTeam.EstimationResult[0].Member!.Type, result.EstimationResult[0].Member!.Type);
        Assert.AreEqual(scrumTeam.EstimationResult[0].Estimation!.Value, result.EstimationResult[0].Estimation!.Value);
        Assert.AreEqual(scrumTeam.EstimationResult[1].Member!.Name, result.EstimationResult[1].Member!.Name);
        Assert.AreEqual(scrumTeam.EstimationResult[1].Member!.Type, result.EstimationResult[1].Member!.Type);
        Assert.AreEqual(scrumTeam.EstimationResult[1].Estimation!.Value, result.EstimationResult[1].Estimation!.Value);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_ScrumTeam_EstimationInProgress(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
        var teamMember = new TeamMember { Name = "John", Type = "Member" };

        var availableEstimations = new List<Estimation>
        {
            new() { Value = 1 },
            new() { Value = 2 }
        };

        var scrumTeam = new ScrumTeam
        {
            Name = "Test Team",
            State = TeamState.EstimationInProgress,
            ScrumMaster = scrumMaster,
            Members = [scrumMaster, teamMember],
            Observers = [],
            AvailableEstimations = availableEstimations,
            EstimationParticipants =
            [
                new EstimationParticipantStatus { MemberName = teamMember.Name, Estimated = true },
                new EstimationParticipantStatus { MemberName = scrumMaster.Name, Estimated = false }
            ]
        };

        var result = SerializeAndDeserialize(scrumTeam, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(scrumTeam.Name, result.Name);
        Assert.AreEqual(scrumTeam.State, result.State);
        Assert.AreEqual(scrumTeam.TimerEndTime, result.TimerEndTime);
        Assert.IsNotNull(result.ScrumMaster);
        Assert.AreEqual(scrumTeam.ScrumMaster.Name, result.ScrumMaster.Name);
        Assert.AreEqual(scrumTeam.ScrumMaster.Type, result.ScrumMaster.Type);
        Assert.AreEqual(scrumTeam.Members[0].Name, result.Members[0].Name);
        Assert.AreEqual(scrumTeam.Members[0].Type, result.Members[0].Type);
        Assert.AreEqual(scrumTeam.Members[1].Name, result.Members[1].Name);
        Assert.AreEqual(scrumTeam.Members[1].Type, result.Members[1].Type);
        Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, result.AvailableEstimations[0].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, result.AvailableEstimations[1].Value);
        Assert.IsNotNull(result.EstimationParticipants);
        Assert.AreEqual(scrumTeam.EstimationParticipants[0].MemberName, result.EstimationParticipants[0].MemberName);
        Assert.AreEqual(scrumTeam.EstimationParticipants[0].Estimated, result.EstimationParticipants[0].Estimated);
        Assert.AreEqual(scrumTeam.EstimationParticipants[1].MemberName, result.EstimationParticipants[1].MemberName);
        Assert.AreEqual(scrumTeam.EstimationParticipants[1].Estimated, result.EstimationParticipants[1].Estimated);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_ReconnectTeamResult_Initial(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };

        var scrumTeam = new ScrumTeam
        {
            Name = "Test Team",
            State = TeamState.Initial,
            ScrumMaster = scrumMaster,
            TimerEndTime = new DateTime(2022, 3, 4, 5, 6, 7, DateTimeKind.Utc),
            Members = [scrumMaster],
            AvailableEstimations =
            [
                new Estimation { Value = null },
                new Estimation { Value = 1 },
                new Estimation { Value = 2 }
            ]
        };

        var reconnectTeamResult = new ReconnectTeamResult
        {
            ScrumTeam = scrumTeam,
            LastMessageId = 0
        };

        var result = SerializeAndDeserialize(reconnectTeamResult, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(reconnectTeamResult.LastMessageId, result.LastMessageId);
        Assert.IsNull(result.SelectedEstimation);

        var resultScrumTeam = result.ScrumTeam;
        Assert.IsNotNull(resultScrumTeam);
        Assert.IsNotNull(resultScrumTeam.ScrumMaster);
        Assert.AreEqual(scrumTeam.Name, resultScrumTeam.Name);
        Assert.AreEqual(scrumTeam.State, resultScrumTeam.State);
        Assert.AreEqual(scrumTeam.TimerEndTime, resultScrumTeam.TimerEndTime);
        Assert.AreEqual(scrumTeam.ScrumMaster.Name, resultScrumTeam.ScrumMaster.Name);
        Assert.AreEqual(scrumTeam.ScrumMaster.Type, resultScrumTeam.ScrumMaster.Type);
        Assert.AreEqual(scrumTeam.Members[0].Name, resultScrumTeam.Members[0].Name);
        Assert.AreEqual(scrumTeam.Members[0].Type, resultScrumTeam.Members[0].Type);
        Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, resultScrumTeam.AvailableEstimations[0].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, resultScrumTeam.AvailableEstimations[1].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[2].Value, resultScrumTeam.AvailableEstimations[2].Value);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_ReconnectTeamResult_EstimationInProgress(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var scrumMaster = new TeamMember { Name = "master", Type = "ScrumMaster" };
        var teamMember = new TeamMember { Name = "John", Type = "Member" };

        var availableEstimations = new List<Estimation>
        {
            new() { Value = 1 },
            new() { Value = 2 }
        };

        var scrumTeam = new ScrumTeam
        {
            Name = "Test Team",
            State = TeamState.EstimationInProgress,
            ScrumMaster = scrumMaster,
            Members = [scrumMaster, teamMember],
            Observers = [],
            AvailableEstimations = availableEstimations,
            EstimationParticipants =
            [
                new EstimationParticipantStatus { MemberName = teamMember.Name, Estimated = true },
                new EstimationParticipantStatus { MemberName = scrumMaster.Name, Estimated = false }
            ]
        };

        var reconnectTeamResult = new ReconnectTeamResult
        {
            ScrumTeam = scrumTeam,
            LastMessageId = 5147483647,
            SelectedEstimation = availableEstimations[1]
        };

        var result = SerializeAndDeserialize(reconnectTeamResult, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(reconnectTeamResult.LastMessageId, result.LastMessageId);
        Assert.IsNotNull(result.SelectedEstimation);
        Assert.AreEqual(reconnectTeamResult.SelectedEstimation.Value, result.SelectedEstimation.Value);

        var resultScrumTeam = result.ScrumTeam;
        Assert.IsNotNull(resultScrumTeam);
        Assert.IsNotNull(resultScrumTeam.ScrumMaster);
        Assert.AreEqual(scrumTeam.Name, resultScrumTeam.Name);
        Assert.AreEqual(scrumTeam.State, resultScrumTeam.State);
        Assert.AreEqual(scrumTeam.TimerEndTime, resultScrumTeam.TimerEndTime);
        Assert.AreEqual(scrumTeam.ScrumMaster.Name, resultScrumTeam.ScrumMaster.Name);
        Assert.AreEqual(scrumTeam.ScrumMaster.Type, resultScrumTeam.ScrumMaster.Type);
        Assert.AreEqual(scrumTeam.Members[0].Name, resultScrumTeam.Members[0].Name);
        Assert.AreEqual(scrumTeam.Members[0].Type, resultScrumTeam.Members[0].Type);
        Assert.AreEqual(scrumTeam.Members[1].Name, resultScrumTeam.Members[1].Name);
        Assert.AreEqual(scrumTeam.Members[1].Type, resultScrumTeam.Members[1].Type);
        Assert.AreEqual(scrumTeam.AvailableEstimations[0].Value, resultScrumTeam.AvailableEstimations[0].Value);
        Assert.AreEqual(scrumTeam.AvailableEstimations[1].Value, resultScrumTeam.AvailableEstimations[1].Value);
        Assert.IsNotNull(resultScrumTeam.EstimationParticipants);
        Assert.AreEqual(scrumTeam.EstimationParticipants[0].MemberName, resultScrumTeam.EstimationParticipants[0].MemberName);
        Assert.AreEqual(scrumTeam.EstimationParticipants[0].Estimated, resultScrumTeam.EstimationParticipants[0].Estimated);
        Assert.AreEqual(scrumTeam.EstimationParticipants[1].MemberName, resultScrumTeam.EstimationParticipants[1].MemberName);
        Assert.AreEqual(scrumTeam.EstimationParticipants[1].Estimated, resultScrumTeam.EstimationParticipants[1].Estimated);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_Empty(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new Message
        {
            Id = 1,
            Type = MessageType.Empty
        };

        var result = SerializeAndDeserialize(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_EstimationStarted(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new Message
        {
            Id = 2,
            Type = MessageType.EstimationStarted
        };

        var result = SerializeAndDeserialize(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_MemberJoined(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new MemberMessage
        {
            Id = 2,
            Type = MessageType.MemberJoined,
            Member = new TeamMember { Name = "master", Type = "ScrumMaster" }
        };

        var result = SerializeAndDeserialize<Message>(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);

        Assert.IsInstanceOfType(result, typeof(MemberMessage));
        var memberMessageResult = (MemberMessage)result;
        Assert.IsNotNull(memberMessageResult.Member);
        Assert.AreEqual(message.Member.Name, memberMessageResult.Member.Name);
        Assert.AreEqual(message.Member.Type, memberMessageResult.Member.Type);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_EstimationEnded(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new EstimationResultMessage
        {
            Id = 10,
            Type = MessageType.EstimationEnded,
            EstimationResult =
            [
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
            ]
        };

        var result = SerializeAndDeserialize<Message>(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);

        Assert.IsInstanceOfType(result, typeof(EstimationResultMessage));
        var estimationResult = (EstimationResultMessage)result;
        Assert.AreEqual(message.EstimationResult[0].Member!.Name, estimationResult.EstimationResult[0].Member!.Name);
        Assert.AreEqual(message.EstimationResult[0].Member!.Type, estimationResult.EstimationResult[0].Member!.Type);
        Assert.AreEqual(message.EstimationResult[0].Estimation!.Value, estimationResult.EstimationResult[0].Estimation!.Value);
        Assert.AreEqual(message.EstimationResult[1].Member!.Name, estimationResult.EstimationResult[1].Member!.Name);
        Assert.AreEqual(message.EstimationResult[1].Member!.Type, estimationResult.EstimationResult[1].Member!.Type);
        Assert.AreEqual(message.EstimationResult[1].Estimation!.Value, estimationResult.EstimationResult[1].Estimation!.Value);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_AvailableEstimationsChanged(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new EstimationSetMessage
        {
            Id = 11,
            Type = MessageType.AvailableEstimationsChanged,
            Estimations =
            [
                new Estimation
                {
                    Value = 0
                },
                new Estimation
                {
                    Value = 0.5
                },
                new Estimation
                {
                    Value = 1
                },
                new Estimation
                {
                    Value = 2
                },
                new Estimation
                {
                    Value = 20
                },
                new Estimation
                {
                    Value = Estimation.PositiveInfinity
                },
                new Estimation()
            ]
        };

        var result = SerializeAndDeserialize<Message>(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);

        Assert.IsInstanceOfType(result, typeof(EstimationSetMessage));
        var estimationSetMessage = (EstimationSetMessage)result;
        Assert.AreEqual(7, estimationSetMessage.Estimations.Count);
        Assert.AreEqual(0.0, estimationSetMessage.Estimations[0].Value);
        Assert.AreEqual(0.5, estimationSetMessage.Estimations[1].Value);
        Assert.AreEqual(1.0, estimationSetMessage.Estimations[2].Value);
        Assert.AreEqual(2.0, estimationSetMessage.Estimations[3].Value);
        Assert.AreEqual(20.0, estimationSetMessage.Estimations[4].Value);
        Assert.AreEqual(Estimation.PositiveInfinity, estimationSetMessage.Estimations[5].Value);
        Assert.IsNull(estimationSetMessage.Estimations[6].Value);
    }

    [DataTestMethod]
    [DynamicData(nameof(JsonTestData))]
    public void JsonSerialize_Message_TimerStarted(JsonSerializerDefaults jsonSerializerDefaults)
    {
        var message = new TimerMessage
        {
            Id = 3,
            Type = MessageType.TimerStarted,
            EndTime = new DateTime(2021, 11, 17, 22, 33, 41, DateTimeKind.Utc)
        };

        var result = SerializeAndDeserialize<Message>(message, jsonSerializerDefaults);

        Assert.IsNotNull(result);
        Assert.AreEqual(message.Id, result.Id);
        Assert.AreEqual(message.Type, result.Type);

        Assert.IsInstanceOfType(result, typeof(TimerMessage));
        var timerMessageResult = (TimerMessage)result;
        Assert.AreEqual(message.EndTime, timerMessageResult.EndTime);
    }

    private static T? SerializeAndDeserialize<T>(T value, JsonSerializerDefaults jsonSerializerDefaults)
    {
        var options = new JsonSerializerOptions(jsonSerializerDefaults);
        var json = JsonSerializer.Serialize(value, options);
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
