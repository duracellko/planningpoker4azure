using System;
using System.Collections.Generic;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    public static class PlanningPokerData
    {
        public const string ScrumMasterType = "ScrumMaster";
        public const string MemberType = "Member";
        public const string ObserverType = "Observer";

        public const string TeamName = "Test team";
        public const string ScrumMasterName = "Test Scrum Master";
        public const string MemberName = "Test member";
        public const string ObserverName = "Test observer";

        public static ScrumTeam GetScrumTeam()
        {
            return new ScrumTeam
            {
                Name = TeamName,
                ScrumMaster = new TeamMember
                {
                    Name = ScrumMasterName,
                    Type = ScrumMasterType
                },
                Members = new List<TeamMember>()
                {
                    new TeamMember
                    {
                        Name = ScrumMasterName,
                        Type = ScrumMasterType
                    },
                    new TeamMember
                    {
                        Name = MemberName,
                        Type = MemberType
                    }
                },
                Observers = new List<TeamMember>()
                {
                    new TeamMember
                    {
                        Name = ObserverName,
                        Type = ObserverType
                    }
                },
                State = TeamState.Initial,
                AvailableEstimations = GetAvailableEstimations()
            };
        }

        public static ScrumTeam GetInitialScrumTeam()
        {
            return new ScrumTeam
            {
                Name = TeamName,
                ScrumMaster = new TeamMember
                {
                    Name = ScrumMasterName,
                    Type = ScrumMasterType
                },
                State = TeamState.Initial,
                AvailableEstimations = GetAvailableEstimations()
            };
        }

        public static IList<Estimation> GetAvailableEstimations()
        {
            return new List<Estimation>
            {
                new Estimation() { Value = 0.0 },
                new Estimation() { Value = 0.5 },
                new Estimation() { Value = 1.0 },
                new Estimation() { Value = 2.0 },
                new Estimation() { Value = 3.0 },
                new Estimation() { Value = 5.0 },
                new Estimation() { Value = 8.0 },
                new Estimation() { Value = 13.0 },
                new Estimation() { Value = 20.0 },
                new Estimation() { Value = 40.0 },
                new Estimation() { Value = 100.0 },
                new Estimation() { Value = double.PositiveInfinity },
                new Estimation() { Value = null }
            };
        }

        public static TeamResult GetTeamResult(ScrumTeam scrumTeam = null)
        {
            return new TeamResult
            {
                ScrumTeam = scrumTeam ?? GetScrumTeam(),
                SessionId = Guid.NewGuid()
            };
        }

        public static ReconnectTeamResult GetReconnectTeamResult()
        {
            return new ReconnectTeamResult
            {
                ScrumTeam = GetScrumTeam(),
                SessionId = Guid.NewGuid(),
                LastMessageId = 123
            };
        }

        public static MemberCredentials GetMemberCredentials()
        {
            return new MemberCredentials
            {
                TeamName = TeamName,
                MemberName = MemberName
            };
        }
    }
}
