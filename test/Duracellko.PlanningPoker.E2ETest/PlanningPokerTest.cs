using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest;

[TestClass]
public class PlanningPokerTest : E2ETestBase
{
    [TestMethod]
    [EnvironmentDataSource]
    public async Task Estimate_2_Rounds(bool serverSide, bool useHttpClient)
    {
        SetupClientsCount = 2;
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "Duracellko.NET";
        var scrumMaster = "Alice";
        var member = "Bob";

        // Alice creates team
        await ClientTest.OpenApplication();
        await TakeScreenshot("01-A-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-A-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster);
        await TakeScreenshot("03-A-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage(team, scrumMaster);
        await TakeScreenshot("04-A-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        // Bob joins team
        await ClientTests[1].OpenApplication();
        await TakeScreenshot(1, "05-B-Loading");
        await ClientTests[1].AssertIndexPage();
        await TakeScreenshot(1, "06-B-Index");
        await ClientTests[1].FillJoinTeamForm(team, member);
        await TakeScreenshot(1, "07-B-JoinTeamForm");
        await ClientTests[1].SubmitJoinTeamForm();
        await ClientTests[1].AssertPlanningPokerPage(team, member);
        await TakeScreenshot(1, "08-B-PlanningPoker");
        await ClientTests[1].AssertTeamName(team, member);
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member);
        await ClientTests[1].AssertObserversInTeam();

        await ClientTest.AssertMembersInTeam(member);

        // Alice starts estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("09-A-EstimationStarted");

        // Bob estimates
        await TakeScreenshot(1, "10-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();
        await ClientTests[1].SelectEstimation("8");

        var expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
        await TakeScreenshot(1, "11-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await ClientTests[1].AssertNotAvailableEstimations();
        await TakeScreenshot("12-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);

        // Alice estimates
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("3");
        expectedResult =
        [
            new KeyValuePair<string, string>(scrumMaster, "3"),
            new KeyValuePair<string, string>(member, "8")
        ];
        await TakeScreenshot("13-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "14-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Alice shows average
        await ClientTest.ShowAverage(true);
        await TakeScreenshot("15-A-ScrumMasterShowsAverage");
        await ClientTest.AssertEstimationSummary(5.5, 5.5, 11);

        // Bob shows average
        await ClientTests[1].ShowAverage(false);
        await TakeScreenshot(1, "16-B-MemberShowsAverage");
        await ClientTests[1].AssertEstimationSummary(5.5, 5.5, 11);

        // Alice starts 2nd round of estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("17-A-EstimationStarted");

        // Alice estimates
        await TakeScreenshot(1, "18-B-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("\u00BD");

        expectedResult = [new KeyValuePair<string, string>(scrumMaster, string.Empty)];
        await TakeScreenshot("19-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await ClientTest.AssertNotAvailableEstimations();
        await TakeScreenshot(1, "20-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Bob estimates
        await ClientTests[1].AssertAvailableEstimations();
        await ClientTests[1].SelectEstimation("\u221E");
        expectedResult =
        [
            new KeyValuePair<string, string>(scrumMaster, "\u00BD"),
            new KeyValuePair<string, string>(member, "\u221E")
        ];
        await TakeScreenshot(1, "21-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot("22-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);

        // Bob disconnects
        await ClientTests[1].Disconnect();
        await TakeScreenshot(1, "23-B-Disconnected");

        await TakeScreenshot("24-A-Disconnected");
        await ClientTest.AssertMembersInTeam();

        // Alice disconnects
        await ClientTest.Disconnect();
        await TakeScreenshot("25-A-Disconnected");
    }

    [TestMethod]
    [EnvironmentDataSource]
    public async Task Cancel_Estimation(bool serverSide, bool useHttpClient)
    {
        SetupClientsCount = 2;
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "Duracellko.NET";
        var scrumMaster = "Alice";
        var member = "Bob";

        // Alice creates team
        await ClientTest.OpenApplication();
        await TakeScreenshot("01-A-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-A-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster);
        await TakeScreenshot("03-A-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage(team, scrumMaster);
        await TakeScreenshot("04-A-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        // Bob joins team
        await ClientTests[1].OpenApplication();
        await TakeScreenshot(1, "05-B-Loading");
        await ClientTests[1].AssertIndexPage();
        await TakeScreenshot(1, "06-B-Index");
        await ClientTests[1].FillJoinTeamForm(team, member);
        await TakeScreenshot(1, "07-B-JoinTeamForm");
        await ClientTests[1].SubmitJoinTeamForm();
        await ClientTests[1].AssertPlanningPokerPage(team, member);
        await TakeScreenshot(1, "08-B-PlanningPoker");
        await ClientTests[1].AssertTeamName(team, member);
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member);
        await ClientTests[1].AssertObserversInTeam();

        await ClientTest.AssertMembersInTeam(member);

        // Alice starts estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("09-A-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();

        await TakeScreenshot(1, "10-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();

        // Alice estimates
        await ClientTest.SelectEstimation("100");
        var expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
        await TakeScreenshot(1, "11-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot("12-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await ClientTest.AssertNotAvailableEstimations();

        // Alice cancels estimation
        await ClientTest.CancelEstimation();

        await TakeScreenshot("13-A-EstimationCancelled");
        await ClientTest.AssertNotAvailableEstimations();
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "14-B-EstimationCancelled");
        await ClientTests[1].AssertNotAvailableEstimations();
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Alice starts estimation again
        await ClientTest.StartEstimation();
        await TakeScreenshot("15-A-EstimationStarted");

        // Alice estimates
        await TakeScreenshot(1, "16-B-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("100");

        expectedResult = [new KeyValuePair<string, string>(scrumMaster, string.Empty)];
        await TakeScreenshot("17-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "18-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Bob estimates
        await ClientTests[1].AssertAvailableEstimations();
        await ClientTests[1].SelectEstimation("20");
        expectedResult =
        [
            new KeyValuePair<string, string>(member, "20"),
            new KeyValuePair<string, string>(scrumMaster, "100")
        ];
        await TakeScreenshot(1, "19-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await ClientTests[1].AssertNotAvailableEstimations();
        await TakeScreenshot("20-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);

        // Alice disconnects
        await ClientTest.Disconnect();
        await TakeScreenshot("21-A-Disconnected");

        await TakeScreenshot(1, "22-B-Disconnected");
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member);

        // Bob disconnects
        await ClientTests[1].Disconnect();
        await TakeScreenshot(1, "23-A-Disconnected");
    }

    [TestMethod]
    [EnvironmentDataSource]
    public async Task Close_Estimation(bool serverSide, bool useHttpClient)
    {
        SetupClientsCount = 2;
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "Duracellko.NET";
        var scrumMaster = "Alice";
        var member = "Bob";

        // Alice creates team
        await ClientTest.OpenApplication();
        await TakeScreenshot("01-A-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-A-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster);
        await TakeScreenshot("03-A-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage(team, scrumMaster);
        await TakeScreenshot("04-A-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        // Bob joins team
        await ClientTests[1].OpenApplication();
        await TakeScreenshot(1, "05-B-Loading");
        await ClientTests[1].AssertIndexPage();
        await TakeScreenshot(1, "06-B-Index");
        await ClientTests[1].FillJoinTeamForm(team, member);
        await TakeScreenshot(1, "07-B-JoinTeamForm");
        await ClientTests[1].SubmitJoinTeamForm();
        await ClientTests[1].AssertPlanningPokerPage(team, member);
        await TakeScreenshot(1, "08-B-PlanningPoker");
        await ClientTests[1].AssertTeamName(team, member);
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member);
        await ClientTests[1].AssertObserversInTeam();

        await ClientTest.AssertMembersInTeam(member);

        // Alice starts estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("09-A-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();

        await TakeScreenshot(1, "10-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();

        // Alice estimates
        await ClientTest.SelectEstimation("8");
        var expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
        await TakeScreenshot(1, "11-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot("12-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await ClientTest.AssertNotAvailableEstimations();

        // Alice closes estimation
        await ClientTest.CloseEstimation();

        await TakeScreenshot("13-A-EstimationClosed");
        expectedResult =
        [
            new KeyValuePair<string, string>(scrumMaster, "8"),
            new KeyValuePair<string, string>(member, string.Empty)
        ];
        await ClientTest.AssertNotAvailableEstimations();
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "14-B-EstimationClosed");
        await ClientTests[1].AssertNotAvailableEstimations();
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Bob disconnects
        await ClientTests[1].Disconnect();
        await TakeScreenshot(1, "15-B-Disconnected");

        await TakeScreenshot("16-A-Disconnected");
        await ClientTest.AssertMembersInTeam();

        // Alice disconnects
        await ClientTest.Disconnect();
        await TakeScreenshot("17-A-Disconnected");
    }

    [TestMethod]
    [EnvironmentDataSource]
    public async Task Observer_Cannot_Estimate(bool serverSide, bool useHttpClient)
    {
        SetupClientsCount = 3;
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "Duracellko.NET";
        var scrumMaster = "Alice";
        var member = "Bob";
        var observer = "Charlie";

        // Alice creates team
        await ClientTest.OpenApplication();
        await TakeScreenshot("01-A-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-A-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster);
        await TakeScreenshot("03-A-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage(team, scrumMaster);
        await TakeScreenshot("04-A-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        // Bob joins team
        await ClientTests[1].OpenApplication();
        await TakeScreenshot(1, "05-B-Loading");
        await ClientTests[1].AssertIndexPage();
        await TakeScreenshot(1, "06-B-Index");
        await ClientTests[1].FillJoinTeamForm(team, member);
        await TakeScreenshot(1, "07-B-JoinTeamForm");
        await ClientTests[1].SubmitJoinTeamForm();
        await ClientTests[1].AssertPlanningPokerPage(team, member);
        await TakeScreenshot(1, "08-B-PlanningPoker");
        await ClientTests[1].AssertTeamName(team, member);
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member);
        await ClientTests[1].AssertObserversInTeam();

        // Charlie joins team as observer
        await ClientTests[2].OpenApplication();
        await TakeScreenshot(2, "09-C-Loading");
        await ClientTests[2].AssertIndexPage();
        await TakeScreenshot(2, "10-C-Index");
        await ClientTests[2].FillJoinTeamForm(team, observer, true);
        await TakeScreenshot(2, "11-C-JoinTeamForm");
        await ClientTests[2].SubmitJoinTeamForm();
        await ClientTests[2].AssertPlanningPokerPage(team, observer);
        await TakeScreenshot(2, "12-C-PlanningPoker");
        await ClientTests[2].AssertTeamName(team, observer);
        await ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[2].AssertMembersInTeam(member);
        await ClientTests[2].AssertObserversInTeam(observer);

        await ClientTest.AssertObserversInTeam(observer);
        await ClientTests[1].AssertObserversInTeam(observer);

        // Alice starts estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("13-A-EstimationStarted");

        await TakeScreenshot(1, "14-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();
        await TakeScreenshot(2, "15-C-EstimationStarted");
        await ClientTests[2].AssertNotAvailableEstimations();

        // Bob estimates
        await ClientTests[1].SelectEstimation("3");
        var expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
        await TakeScreenshot(1, "16-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot("17-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "16-C-MemberEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Alice estimates
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("2");
        expectedResult =
        [
            new KeyValuePair<string, string>(scrumMaster, "2"),
            new KeyValuePair<string, string>(member, "3")
        ];
        await TakeScreenshot("17-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "18-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "19-C-ScrumMasterEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Bob disconnects
        await ClientTests[1].Disconnect();
        await TakeScreenshot(1, "20-B-Disconnected");

        await TakeScreenshot("21-A-Disconnected");
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam(observer);
        await TakeScreenshot(2, "22-C-Disconnected");
        await ClientTests[2].AssertMembersInTeam();
        await ClientTests[2].AssertObserversInTeam(observer);

        // Alice disconnects
        await ClientTest.Disconnect();
        await TakeScreenshot("23-A-Disconnected");

        await TakeScreenshot(2, "24-C-Disconnected");
        await ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[2].AssertMembersInTeam();
        await ClientTests[2].AssertObserversInTeam(observer);

        // Charlie disconnects
        await ClientTests[2].Disconnect();
        await TakeScreenshot("25-C-Disconnected");
    }

    [TestMethod]
    [EnvironmentDataSource]
    public async Task Cannot_Estimate_When_Joining_After_Start(bool serverSide, bool useHttpClient)
    {
        SetupClientsCount = 3;
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "Duracellko.NET";
        var scrumMaster = "Alice";
        var member1 = "Bob";
        var member2 = "Charlie";

        // Alice creates team
        await ClientTest.OpenApplication();
        await TakeScreenshot("01-A-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-A-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster);
        await TakeScreenshot("03-A-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage(team, scrumMaster);
        await TakeScreenshot("04-A-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        // Bob joins team
        await ClientTests[1].OpenApplication();
        await TakeScreenshot(1, "05-B-Loading");
        await ClientTests[1].AssertIndexPage();
        await TakeScreenshot(1, "06-B-Index");
        await ClientTests[1].FillJoinTeamForm(team, member1);
        await TakeScreenshot(1, "07-B-JoinTeamForm");
        await ClientTests[1].SubmitJoinTeamForm();
        await ClientTests[1].AssertPlanningPokerPage(team, member1);
        await TakeScreenshot(1, "08-B-PlanningPoker");
        await ClientTests[1].AssertTeamName(team, member1);
        await ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[1].AssertMembersInTeam(member1);
        await ClientTests[1].AssertObserversInTeam();

        await ClientTest.AssertMembersInTeam(member1);

        // Alice starts estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("09-A-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();

        await TakeScreenshot(1, "10-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();

        // Charlie joins team
        await ClientTests[2].OpenApplication();
        await TakeScreenshot(2, "11-C-Loading");
        await ClientTests[2].AssertIndexPage();
        await TakeScreenshot(2, "12-C-Index");
        await ClientTests[2].FillJoinTeamForm(team, member2);
        await TakeScreenshot(2, "13-C-JoinTeamForm");
        await ClientTests[2].SubmitJoinTeamForm();
        await ClientTests[2].AssertPlanningPokerPage(team, member2);
        await TakeScreenshot(2, "14-C-PlanningPoker");
        await ClientTests[2].AssertTeamName(team, member2);
        await ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
        await ClientTests[2].AssertMembersInTeam(member1, member2);
        await ClientTests[2].AssertObserversInTeam();

        await ClientTest.AssertMembersInTeam(member1, member2);
        await TakeScreenshot("15-A-MemberJoiner");
        await ClientTests[1].AssertMembersInTeam(member1, member2);
        await TakeScreenshot(1, "16-B-MemberJoiner");
        await ClientTests[2].AssertNotAvailableEstimations();

        // Bob estimates
        await ClientTests[1].SelectEstimation("13");
        var expectedResult = new[] { new KeyValuePair<string, string>(member1, string.Empty) };
        await TakeScreenshot(1, "17-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot("18-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "19-C-MemberEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Alice estimates
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("20");
        expectedResult =
        [
            new KeyValuePair<string, string>(member1, "13"),
            new KeyValuePair<string, string>(scrumMaster, "20")
        ];
        await TakeScreenshot("20-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "21-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "22-C-ScrumMasterEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Alice starts 2nd round of estimation
        await ClientTest.StartEstimation();
        await TakeScreenshot("23-A-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();

        await TakeScreenshot(1, "24-B-EstimationStarted");
        await ClientTests[1].AssertAvailableEstimations();
        await TakeScreenshot(2, "25-C-EstimationStarted");
        await ClientTests[2].AssertAvailableEstimations();

        // Charlie estimates
        await ClientTests[2].SelectEstimation("20");
        expectedResult = [new KeyValuePair<string, string>(member2, string.Empty)];
        await TakeScreenshot(2, "26-C-MemberEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);
        await ClientTests[2].AssertNotAvailableEstimations();
        await TakeScreenshot("27-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(1, "28-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);

        // Alice estimates
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("20");
        expectedResult =
        [
            new KeyValuePair<string, string>(member2, string.Empty),
            new KeyValuePair<string, string>(scrumMaster, string.Empty)
        ];
        await TakeScreenshot("29-A-ScrumMasterEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await ClientTest.AssertNotAvailableEstimations();
        await TakeScreenshot(1, "30-B-ScrumMasterEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "31-C-ScrumMasterEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Bob estimates
        await ClientTests[1].AssertAvailableEstimations();
        await ClientTests[1].SelectEstimation("2");
        expectedResult =
        [
            new KeyValuePair<string, string>(scrumMaster, "20"),
            new KeyValuePair<string, string>(member2, "20"),
            new KeyValuePair<string, string>(member1, "2")
        ];
        await TakeScreenshot(1, "32-B-MemberEstimated");
        await ClientTests[1].AssertSelectedEstimation(expectedResult);
        await ClientTests[1].AssertNotAvailableEstimations();
        await TakeScreenshot("33-A-MemberEstimated");
        await ClientTest.AssertSelectedEstimation(expectedResult);
        await TakeScreenshot(2, "34-C-MemberEstimated");
        await ClientTests[2].AssertSelectedEstimation(expectedResult);

        // Bob diconnects
        await ClientTests[1].Disconnect();
        await TakeScreenshot(1, "35-B-Disconnected");

        await TakeScreenshot("36-A-Disconnected");
        await ClientTest.AssertMembersInTeam(member2);
        await TakeScreenshot(2, "37-C-Disconnected");
        await ClientTests[2].AssertMembersInTeam(member2);

        // Charlie disconnects
        await ClientTests[2].Disconnect();
        await TakeScreenshot(2, "38-C-Disconnected");

        await TakeScreenshot("39-A-Disconnected");
        await ClientTest.AssertMembersInTeam();

        // Alice disconnects
        await ClientTest.Disconnect();
        await TakeScreenshot("40-A-Disconnected");
    }
}
