using System;
using System.Collections.Generic;
using System.Text;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Test.Service;

public static class PlanningPokerData
{
    public const string ScrumMasterType = "ScrumMaster";
    public const string MemberType = "Member";
    public const string ObserverType = "Observer";

    public const string TeamName = "Test team";
    public const string ScrumMasterName = "Test Scrum Master";
    public const string MemberName = "Test member";
    public const string ObserverName = "Test observer";

    public static readonly Guid SessionId = Guid.NewGuid();

    public static ScrumTeam GetScrumTeam(bool member = false, bool observer = false, TeamState state = TeamState.Initial, IList<EstimationResultItem>? estimationResult = null, IList<EstimationParticipantStatus>? estimationParticipants = null)
    {
        var result = new ScrumTeam
        {
            Name = TeamName,
            ScrumMaster = new TeamMember { Name = ScrumMasterName, Type = ScrumMasterType },
            Members = new List<TeamMember>
            {
                new TeamMember { Name = ScrumMasterName, Type = ScrumMasterType }
            },
            Observers = new List<TeamMember>(),
            State = state,
            AvailableEstimations = new List<Estimation>
            {
                new Estimation { Value = 0 },
                new Estimation { Value = 0.5 },
                new Estimation { Value = 1 },
                new Estimation { Value = 2 },
                new Estimation { Value = 3 },
                new Estimation { Value = 5 },
                new Estimation { Value = 8 },
                new Estimation { Value = 13 },
                new Estimation { Value = 20 },
                new Estimation { Value = 40 },
                new Estimation { Value = 100 },
                new Estimation { Value = Estimation.PositiveInfinity },
                new Estimation(),
            },
            EstimationResult = estimationResult ?? new List<EstimationResultItem>(),
            EstimationParticipants = estimationParticipants ?? new List<EstimationParticipantStatus>()
        };

        if (member)
        {
            result.Members.Add(new TeamMember { Name = MemberName, Type = MemberType });
        }

        if (observer)
        {
            result.Observers.Add(new TeamMember { Name = ObserverName, Type = ObserverType });
        }

        return result;
    }

    public static IList<EstimationResultItem> GetEstimationResult(double? scrumMasterEstimation = 5, double? memberEstimation = 20)
    {
        return new List<EstimationResultItem>
        {
            new EstimationResultItem
            {
                Member = new TeamMember { Name = ScrumMasterName, Type = ScrumMasterType },
                Estimation = GetEstimation(scrumMasterEstimation)
            },
            new EstimationResultItem
            {
                Member = new TeamMember { Name = MemberName, Type = MemberType },
                Estimation = GetEstimation(memberEstimation)
            }
        };
    }

    public static IList<EstimationParticipantStatus> GetEstimationParticipants(bool scrumMaster = true, bool member = false)
    {
        return new List<EstimationParticipantStatus>
        {
            new EstimationParticipantStatus { MemberName = ScrumMasterName, Estimated = scrumMaster },
            new EstimationParticipantStatus { MemberName = MemberName, Estimated = member }
        };
    }

    public static TeamResult GetTeamResult(ScrumTeam scrumTeam)
    {
        return new TeamResult
        {
            ScrumTeam = scrumTeam,
            SessionId = Guid.NewGuid()
        };
    }

    public static ReconnectTeamResult GetReconnectTeamResult(ScrumTeam scrumTeam, long lastMessageId = 0, double? selectedEstimation = double.NaN)
    {
        return new ReconnectTeamResult
        {
            ScrumTeam = scrumTeam,
            SessionId = Guid.NewGuid(),
            LastMessageId = lastMessageId,
            SelectedEstimation = GetEstimation(selectedEstimation)
        };
    }

    private static Estimation? GetEstimation(double? estimation)
    {
        if (estimation.HasValue)
        {
            return double.IsNaN(estimation.Value) ? null : new Estimation { Value = estimation };
        }
        else
        {
            return new Estimation();
        }
    }
}
