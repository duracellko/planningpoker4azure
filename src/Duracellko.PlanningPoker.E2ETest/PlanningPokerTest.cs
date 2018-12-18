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
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Estimate_2_Rounds(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Bob";
            string member = "Alice";

            ClientTest.OpenApplication();
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

            ClientTests[1].OpenApplication();
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

            ClientTest.StartEstimation();
            TakeScreenshot("09-A-EstimationStarted");

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("\u221E");

            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
            TakeScreenshot(1, "11-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("12-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

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

            ClientTest.StartEstimation();
            TakeScreenshot("15-A-EstimationStarted");

            await Task.Delay(200);
            TakeScreenshot(1, "16-B-EstimationStarted");
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("5");

            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            ClientTests[1].AssertAvailableEstimations();
            ClientTests[1].SelectEstimation("5");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member, "5"),
                new KeyValuePair<string, string>(scrumMaster, "5")
            };
            TakeScreenshot(1, "19-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            ClientTests[1].Disconnect();
            TakeScreenshot(1, "21-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("22-A-Disconnected");
            ClientTest.AssertMembersInTeam();

            ClientTest.Disconnect();
            TakeScreenshot("23-A-Disconnected");
        }

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Cancel_Estimation(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimation),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimation),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Bob";
            string member = "Alice";

            ClientTest.OpenApplication();
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

            ClientTests[1].OpenApplication();
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

            ClientTest.StartEstimation();
            TakeScreenshot("09-A-EstimationStarted");
            ClientTest.AssertAvailableEstimations();

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimationStarted");
            ClientTests[1].AssertAvailableEstimations();

            ClientTest.SelectEstimation("100");
            await Task.Delay(500);
            var expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot(1, "11-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimation(expectedResult);
            TakeScreenshot("12-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            ClientTest.CancelEstimation();
            await Task.Delay(200);

            TakeScreenshot("13-A-EstimationCancelled");
            ClientTest.AssertNotAvailableEstimations();
            ClientTest.AssertSelectedEstimation(expectedResult);
            TakeScreenshot(1, "14-B-EstimationCancelled");
            ClientTests[1].AssertNotAvailableEstimations();
            ClientTests[1].AssertSelectedEstimation(expectedResult);

            ClientTest.StartEstimation();
            TakeScreenshot("15-A-EstimationStarted");

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
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimation(expectedResult);

            ClientTest.Disconnect();
            TakeScreenshot("21-A-Disconnected");

            await Task.Delay(200);
            TakeScreenshot(1, "22-B-Disconnected");
            ClientTests[1].AssertScrumMasterInTeam(string.Empty);
            ClientTests[1].AssertMembersInTeam(member);

            ClientTests[1].Disconnect();
            TakeScreenshot(1, "23-A-Disconnected");
        }
    }
}
