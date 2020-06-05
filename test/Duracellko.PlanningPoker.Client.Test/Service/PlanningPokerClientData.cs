using System;
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

        public static readonly Guid SessionId = Guid.NewGuid();
        public static readonly Guid ReconnectSessionId = Guid.NewGuid();

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

        public static string GetTeamResultJson(string scrumTeamJson)
        {
            return @"{
            ""sessionId"": """ + SessionId.ToString() + @""",
            ""scrumTeam"": " + scrumTeamJson + @"
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
            ""sessionId"": """ + ReconnectSessionId.ToString() + @""",
            ""scrumTeam"": " + scrumTeamJson + selectedEstimationJson + @"
}";
        }

        public static string GetMessagesJson(params string[] messages)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            sb.AppendJoin(",\r\n", messages);
            sb.AppendLine();
            sb.Append(']');
            return sb.ToString();
        }

        public static string GetEmptyMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 0
}";
        }

        public static string GetMemberJoinedMessageJson(string id = "0", string name = MemberName, string type = MemberType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 1,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }

        public static string GetMemberDisconnectedMessageJson(string id = "0", string name = MemberName, string type = MemberType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 2,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }

        public static string GetEstimationStartedMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 3
}";
        }

        public static string GetEstimationEndedMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 4,
            ""estimationResult"": [
                {
                    ""member"": {
                        ""name"": ""Test Scrum Master"",
                        ""type"": ""ScrumMaster""
                    },
                    ""estimation"": {
                        ""value"": 2
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Test member"",
                        ""type"": ""Member""
                    },
                    ""estimation"": {
                        ""value"": null
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Me"",
                        ""type"": ""Member""
                    },
                    ""estimation"": null
                },
                {
                    ""member"": {
                        ""name"": ""Test observer"",
                        ""type"": ""Member""
                    },
                    ""estimation"": {
                        ""value"": -1111100
                    }
                }
            ]
}";
        }

        public static string GetEstimationEndedMessage2Json(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 4,
            ""estimationResult"": [
                {
                    ""member"": {
                        ""name"": ""Test Scrum Master"",
                        ""type"": ""ScrumMaster""
                    },
                    ""estimation"": {
                        ""value"": 5
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Test member"",
                        ""type"": ""Member""
                    },
                    ""estimation"": {
                        ""value"": 40
                    }
                }
            ]
}";
        }

        public static string GetEstimationCanceledMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 5
}";
        }

        public static string GetMemberEstimatedMessageJson(string id = "0", string name = ScrumMasterName, string type = ScrumMasterType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 6,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }
    }
}
