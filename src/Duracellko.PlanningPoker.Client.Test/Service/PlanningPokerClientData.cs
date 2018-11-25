using System.Globalization;
using System.Text;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    public static class PlanningPokerClientData
    {
        public const string ScrumMasterType = "ScrumMaster";
        public const string MemberType = "Member";
        public const string ObserverType = "Observer";

        public const string TeamName = "Test team";
        public const string ScrumMasterName = "Test Scrum Master";
        public const string MemberName = "Test member";
        public const string ObserverName = "Test observer";

        public static string GetScrumTeamJson(bool member = false, bool observer = false, int state = 0, string estimationResult = "", string estimationParticipants = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(@"""name"": ""Test team"",");
            sb.AppendLine(@"    ""scrumMaster"": {
        ""name"": ""Test Scrum Master"",
        ""type"": ""ScrumMaster""
    },
");
            sb.AppendLine(@"    ""members"": [");
            sb.Append(@"        {
            ""name"": ""Test Scrum Master"",
            ""type"": ""ScrumMaster""
        }");
            if (member)
            {
                sb.Append(',');
            }

            sb.AppendLine();

            if (member)
            {
                sb.AppendLine(@"        {
            ""name"": ""Test member"",
            ""type"": ""Member""
        }");
            }

            sb.AppendLine("],");

            sb.AppendLine(@"    ""observers"": [");
            if (observer)
            {
                sb.AppendLine(@"        {
            ""name"": ""Test observer"",
            ""type"": ""Observer""
        }");
            }

            sb.AppendLine("],");

            sb.AppendFormat(CultureInfo.InvariantCulture, @"    ""state"": {0},", state);
            sb.AppendLine();

            sb.AppendLine(@"    ""availableEstimations"": [
        { ""value"": 0 },
        { ""value"": 0.5 },
        { ""value"": 1 },
        { ""value"": 2 },
        { ""value"": 3 },
        { ""value"": 5 },
        { ""value"": 8 },
        { ""value"": 13 },
        { ""value"": 20 },
        { ""value"": 40 },
        { ""value"": 100 },
        { ""value"": -1111100 },
        { ""value"": null }
    ],");

            sb.AppendLine(@"    ""estimationResult"": [");
            sb.AppendLine(estimationResult);
            sb.AppendLine("],");

            sb.AppendLine(@"    ""estimationParticipants"": [");
            sb.AppendLine(estimationParticipants);
            sb.AppendLine("]");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GetEstimationResultJson(string scrumMasterEstimation = "5", string memberEstimation = "20")
        {
            var scrumMasterEstimationJson = string.Empty;
            if (scrumMasterEstimation != null)
            {
                scrumMasterEstimationJson = @",
            ""estimation"": {
                ""value"": " + scrumMasterEstimation + @"
            }";
            }

            var memberEstimationJson = string.Empty;
            if (memberEstimation != null)
            {
                memberEstimationJson = @",
            ""estimation"": {
                ""value"": " + memberEstimation + @"
            }";
            }

            return @"{
            ""member"": {
                ""name"": ""Test Scrum Master"",
                ""type"": ""ScrumMaster""
            }" + scrumMasterEstimationJson + @"
        },
        {
            ""member"": {
                ""name"": ""Test member"",
                ""type"": ""Member""
            }" + memberEstimationJson + @"
        }";
        }

        public static string GetEstimationParticipantsJson(bool scrumMaster = true, bool member = false)
        {
            return @"{
                ""memberName"": ""Test Scrum Master"",
                ""estimated"": " + (scrumMaster ? "true" : "false") + @"
            },
            {
                ""memberName"": ""Test member"",
                ""estimated"": " + (member ? "true" : "false") + @"
            }";
        }

        public static string GetReconnectTeamResultJson(string scrumTeamJson, string lastMessageId = "0", string selectedEstimation = null)
        {
            var selectedEstimationJson = string.Empty;
            if (selectedEstimation != null)
            {
                selectedEstimationJson = @",
                ""selectedEstimation"": {
                    ""value"": " + selectedEstimation + @"
                }";
            }

            return @"{
            ""lastMessageId"": " + lastMessageId + @",
            ""scrumTeam"": " + scrumTeamJson + selectedEstimationJson + @"
}";
        }
    }
}
