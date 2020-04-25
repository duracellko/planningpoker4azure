using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest
{
    [TestClass]
    public class PlanningPokerTest : E2ETestBase
    {
        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Estimate_2_Rounds(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member);

            // Alice starts estimation
            ClientTest.StartEstimation();
            TakeScreenshot("09-A-EstimationStarted");

            // Bob estimates
            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("\u221E");

            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
            TakeScreenshot(1, "11-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            ClientTests[1].AssertNotAvailableEstimations();
            TakeScreenshot("12-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("\u00BD");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "\u00BD"),
                new KeyValuePair<string, string>(member, "\u221E")
            };
            TakeScreenshot("13-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "14-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            // Alice starts 2nd round of estimation
            ClientTest.StartEstimation();
            TakeScreenshot("15-A-EstimationStarted");

            // Alice estimates
            await Task.Delay(200);
            TakeScreenshot(1, "16-B-EstimationStarted");
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("5");

            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            ClientTest.AssertNotAvailableEstimations();
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("5");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "5"),
                new KeyValuePair<string, string>(member, "5")
            };
            TakeScreenshot(1, "19-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "21-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("22-A-Disconnected");
            ClientTest.AssertMembersInTeam();

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("23-A-Disconnected");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Cancel_Estimation(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimation),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimation),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member);

            // Alice starts estimation
            ClientTest.StartEstimation();
            TakeScreenshot("09-A-EstimationStarted");
            ClientTest.AssertAvailableEstimations();

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();

            // Alice estimates
            ClientTest.SelectEstimation("100");
            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot(1, "11-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("12-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            ClientTest.AssertNotAvailableEstimations();

            // Alice cancels estimation
            ClientTest.CancelEstimation();
            await Task.Delay(200);

            TakeScreenshot("13-A-EstimationCancelled");
            ClientTest.AssertNotAvailableEstimations();
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "14-B-EstimationCancelled");
            ClientTests[1].AssertNotAvailableEstimations();
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            // Alice starts estimation again
            ClientTest.StartEstimation();
            TakeScreenshot("15-A-EstimationStarted");

            // Alice estimates
            await Task.Delay(200);
            TakeScreenshot(1, "16-B-EstimationStarted");
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("100");

            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member, "20"),
                new KeyValuePair<string, string>(scrumMaster, "100")
            };
            TakeScreenshot(1, "19-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            ClientTests[1].AssertNotAvailableEstimations();
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("21-A-Disconnected");

            await Task.Delay(200);
            TakeScreenshot(1, "22-B-Disconnected");
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "23-A-Disconnected");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Observer_Cannot_Estimate(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";
            string observer = "Charlie";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            // Charlie joins team as observer
            await ClientTests[2].OpenApplication();
            TakeScreenshot(2, "09-C-Loading");
            ClientTests[2].AssertIndexPage();
            TakeScreenshot(2, "10-C-Index");
            ClientTests[2].FillJoinTeamForm(team, observer, true);
            TakeScreenshot(2, "11-C-JoinTeamForm");
            ClientTests[2].SubmitJoinTeamForm();
            ClientTests[2].AssertPlanningPokerPage(team, observer);
            TakeScreenshot(2, "12-C-PlanningPoker");
            ClientTests[2].AssertTeamName(team, observer);
            ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[2].AssertMembersInTeam(member);
            ClientTests[2].AssertObserversInTeam(observer);

            await Task.Delay(200);
            ClientTest.AssertObserversInTeam(observer);
            ClientTests[1].AssertObserversInTeam(observer);

            // Alice starts estimation
            ClientTest.StartEstimation();
            TakeScreenshot("13-A-EstimationStarted");

            await Task.Delay(200);
            TakeScreenshot(1, "14-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();
            TakeScreenshot(2, "15-C-EstimationStarted");
            ClientTests[2].AssertNotAvailableEstimations();

            // Bob estimates
            ClientTests[1].SelectEstimation("3");
            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
            TakeScreenshot(1, "16-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("17-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "16-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("2");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "2"),
                new KeyValuePair<string, string>(member, "3")
            };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "19-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "20-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("21-A-Disconnected");
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam(observer);
            TakeScreenshot(2, "22-C-Disconnected");
            ClientTests[2].AssertMembersInTeam();
            ClientTests[2].AssertObserversInTeam(observer);

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("23-A-Disconnected");

            await Task.Delay(200);
            TakeScreenshot(2, "24-C-Disconnected");
            ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[2].AssertMembersInTeam();
            ClientTests[2].AssertObserversInTeam(observer);

            // Charlie disconnects
            ClientTests[2].Disconnect();
            TakeScreenshot("25-C-Disconnected");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Cannot_Estimate_When_Joining_After_Start(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType,
                serverSide,
                useHttpClient));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member1 = "Bob";
            string member2 = "Charlie";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member1);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member1);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member1);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member1);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member1);

            // Alice starts estimation
            ClientTest.StartEstimation();
            TakeScreenshot("09-A-EstimationStarted");
            ClientTest.AssertAvailableEstimations();

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();

            // Charlie joins team
            await ClientTests[2].OpenApplication();
            TakeScreenshot(2, "11-C-Loading");
            ClientTests[2].AssertIndexPage();
            TakeScreenshot(2, "12-C-Index");
            ClientTests[2].FillJoinTeamForm(team, member2);
            TakeScreenshot(2, "13-C-JoinTeamForm");
            ClientTests[2].SubmitJoinTeamForm();
            ClientTests[2].AssertPlanningPokerPage(team, member2);
            TakeScreenshot(2, "14-C-PlanningPoker");
            ClientTests[2].AssertTeamName(team, member2);
            ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[2].AssertMembersInTeam(member1, member2);
            ClientTests[2].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member1, member2);
            TakeScreenshot("15-A-MemberJoiner");
            ClientTests[1].AssertMembersInTeam(member1, member2);
            TakeScreenshot(1, "16-B-MemberJoiner");
            ClientTests[2].AssertNotAvailableEstimations();

            // Bob estimates
            ClientTests[1].SelectEstimation("13");
            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(member1, string.Empty) };
            TakeScreenshot(1, "17-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("18-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "19-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member1, "13"),
                new KeyValuePair<string, string>(scrumMaster, "20")
            };
            TakeScreenshot("20-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "21-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "22-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Alice starts 2nd round of estimation
            ClientTest.StartEstimation();
            TakeScreenshot("23-A-EstimationStarted");
            ClientTest.AssertAvailableEstimations();

            await Task.Delay(200);
            TakeScreenshot(1, "24-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();
            TakeScreenshot(2, "25-C-EstimationStarted");
            ClientTests[2].AssertAvailableEstimations();

            // Charlie estimates
            ClientTests[2].SelectEstimation("20");
            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(member2, string.Empty) };
            TakeScreenshot(2, "26-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);
            ClientTests[2].AssertNotAvailableEstimations();
            TakeScreenshot("27-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "28-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member2, string.Empty),
                new KeyValuePair<string, string>(scrumMaster, string.Empty)
            };
            TakeScreenshot("29-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            ClientTest.AssertNotAvailableEstimations();
            TakeScreenshot(1, "30-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "31-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("2");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "20"),
                new KeyValuePair<string, string>(member2, "20"),
                new KeyValuePair<string, string>(member1, "2")
            };
            TakeScreenshot(1, "32-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            ClientTests[1].AssertNotAvailableEstimations();
            TakeScreenshot("33-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(2, "34-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimation(expectedResult);

            // Bob diconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "35-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("36-A-Disconnected");
            ClientTest.AssertMembersInTeam(member2);
            TakeScreenshot(2, "37-C-Disconnected");
            ClientTests[2].AssertMembersInTeam(member2);

            // Charlie disconnects
            ClientTests[2].Disconnect();
            TakeScreenshot(2, "38-C-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("39-A-Disconnected");
            ClientTest.AssertMembersInTeam();

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("40-A-Disconnected");
        }
    }
}
