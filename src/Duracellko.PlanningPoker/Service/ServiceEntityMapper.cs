using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Maps planning poker domain entities to planning poker service data entities.
/// </summary>
internal static class ServiceEntityMapper
{
    /// <summary>
    /// Maps a domain <see cref="D.ScrumTeam"/> entity to a service <see cref="ScrumTeam"/> data object.
    /// </summary>
    /// <param name="team">The domain Scrum Team entity to map.</param>
    /// <returns>The mapped service Scrum Team data object.</returns>
    public static ScrumTeam Map(D.ScrumTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);

        var result = new ScrumTeam
        {
            Name = team.Name,
            State = (TeamState)team.State,
            ScrumMaster = MapTeamMember(team.ScrumMaster),
            EstimationResult = team.EstimationResult?.Select(MapEstimationResultItem).ToList(),
            EstimationParticipants = team.EstimationParticipants?.Select(MapEstimationParticipantStatus).ToList(),
            TimerEndTime = team.TimerEndTime
        };

        foreach (var member in team.Members)
        {
            result.Members.Add(MapTeamMember(member));
        }

        foreach (var member in team.Observers)
        {
            result.Observers.Add(MapTeamMember(member));
        }

        foreach (var estimation in team.AvailableEstimations)
        {
            result.AvailableEstimations.Add(Map(estimation));
        }

        return result;
    }

    /// <summary>
    /// Maps a domain <see cref="D.Message"/> entity to a service <see cref="Message"/> data object.
    /// </summary>
    /// <param name="message">The domain message entity to map.</param>
    /// <returns>The mapped service message data object.</returns>
    public static Message Map(D.Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return message switch
        {
            D.MemberMessage memberMsg => MapMemberMessage(memberMsg),
            D.EstimationResultMessage estimationResultMsg => MapEstimationResultMessage(estimationResultMsg),
            D.EstimationSetMessage estimationSetMsg => MapEstimationSetMessage(estimationSetMsg),
            D.TimerMessage timerMsg => MapTimerMessage(timerMsg),
            _ => MapMessage(message)
        };
    }

    /// <summary>
    /// Maps a domain <see cref="D.Estimation"/> entity to a service <see cref="Estimation"/> data object.
    /// </summary>
    /// <param name="estimation">The domain estimation entity to map.</param>
    /// <returns>The mapped service estimation data object.</returns>
    [return: NotNullIfNotNull(nameof(estimation))]
    public static Estimation? Map(D.Estimation? estimation)
    {
        if (estimation == null)
        {
            return null;
        }

        return new Estimation
        {
            Value = estimation.Value
        };
    }

    /// <summary>
    /// Maps service Deck value to domain Deck value.
    /// </summary>
    /// <param name="value">Service deck value.</param>
    /// <returns>Domain deck value.</returns>
    public static D.Deck Map(Deck value) => (D.Deck)value;

    /// <summary>
    /// Maps <see cref="D.PlanningPokerException"/> to error data object.
    /// </summary>
    /// <param name="exception">The Planning Poker Exception to convert.</param>
    /// <returns>The Planning Poker application error data object.</returns>
    public static PlanningPokerExceptionData Map(D.PlanningPokerException exception)
    {
        return new PlanningPokerExceptionData
        {
            Error = exception.Error,
            Message = exception.Message,
            Argument = exception.Argument
        };
    }

    /// <summary>
    /// Filters or transforms message before sending to client.
    /// MemberDisconnected message of ScrumMaster is transformed to Empty message,
    /// because that is internal message and ScrumMaster is not actually disconnected.
    /// </summary>
    /// <param name="message">The message to transform.</param>
    /// <returns>The transformed message.</returns>
    public static D.Message FilterMessage(D.Message message)
    {
        if (message.MessageType == D.MessageType.MemberDisconnected)
        {
            var memberMessage = (D.MemberMessage)message;
            if (memberMessage.Member is D.ScrumMaster)
            {
                return new D.Message(D.MessageType.Empty, message.Id);
            }
        }

        return message;
    }

    [return: NotNullIfNotNull(nameof(member))]
    private static TeamMember? MapTeamMember(D.Observer? member)
    {
        if (member == null)
        {
            return null;
        }

        return new TeamMember
        {
            Name = member.Name,
            Type = member.GetType().Name
        };
    }

    private static EstimationResultItem MapEstimationResultItem(KeyValuePair<D.Member, D.Estimation?> item)
    {
        return new EstimationResultItem
        {
            Member = MapTeamMember(item.Key),
            Estimation = Map(item.Value)
        };
    }

    private static EstimationParticipantStatus MapEstimationParticipantStatus(D.EstimationParticipantStatus value)
    {
        return new EstimationParticipantStatus
        {
            MemberName = value.MemberName,
            Estimated = value.Estimated
        };
    }

    private static void MapBaseMessage(D.Message source, Message target)
    {
        target.Id = source.Id;
        target.Type = MapMessageType(source.MessageType);
    }

    private static Message MapMessage(D.Message message)
    {
        var result = new Message();
        MapBaseMessage(message, result);
        return result;
    }

    private static MemberMessage MapMemberMessage(D.MemberMessage message)
    {
        var result = new MemberMessage();
        MapBaseMessage(message, result);
        result.Member = MapTeamMember(message.Member);
        return result;
    }

    private static EstimationResultMessage MapEstimationResultMessage(D.EstimationResultMessage message)
    {
        var result = new EstimationResultMessage();
        MapBaseMessage(message, result);

        if (message.EstimationResult != null)
        {
            foreach (var item in message.EstimationResult)
            {
                result.EstimationResult.Add(MapEstimationResultItem(item));
            }
        }

        return result;
    }

    private static EstimationSetMessage MapEstimationSetMessage(D.EstimationSetMessage message)
    {
        var result = new EstimationSetMessage();
        MapBaseMessage(message, result);

        if (message.Estimations != null)
        {
            foreach (var estimation in message.Estimations)
            {
                result.Estimations.Add(Map(estimation));
            }
        }

        return result;
    }

    private static TimerMessage MapTimerMessage(D.TimerMessage message)
    {
        var result = new TimerMessage();
        MapBaseMessage(message, result);
        result.EndTime = message.EndTime;
        return result;
    }

    private static MessageType MapMessageType(D.MessageType messageType)
    {
        return messageType switch
        {
            D.MessageType.Empty => MessageType.Empty,
            D.MessageType.MemberJoined => MessageType.MemberJoined,
            D.MessageType.MemberDisconnected => MessageType.MemberDisconnected,
            D.MessageType.EstimationStarted => MessageType.EstimationStarted,
            D.MessageType.EstimationEnded => MessageType.EstimationEnded,
            D.MessageType.EstimationCanceled => MessageType.EstimationCanceled,
            D.MessageType.MemberEstimated => MessageType.MemberEstimated,
            D.MessageType.AvailableEstimationsChanged => MessageType.AvailableEstimationsChanged,
            D.MessageType.TimerStarted => MessageType.TimerStarted,
            D.MessageType.TimerCanceled => MessageType.TimerCanceled,
            D.MessageType.MemberActivity => MessageType.Empty,
            D.MessageType.TeamCreated => MessageType.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, "Unknown message type.")
        };
    }
}
