using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest;

[TestClass]
public class ScrumMasterTest : E2ETestBase
{
    [DataTestMethod]
    [EnvironmentDataSource]
    public async Task ScrumMaster_Should_Be_Able_To_Estimate(bool serverSide, bool useHttpClient)
    {
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "My team";
        var scrumMaster = "Test ScrumMaster";
        var deckText = "0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100";

        await ClientTest.OpenApplication();
        await TakeScreenshot("01-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster, "Standard", deckText);
        await TakeScreenshot("03-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage("My%20team", "Test%20ScrumMaster");
        await TakeScreenshot("04-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();
        await ClientTest.StartEstimation();
        await TakeScreenshot("05-EstimationStarted");
        await ClientTest.AssertAvailableEstimations();
        await ClientTest.SelectEstimation("1");
        await TakeScreenshot("06-Estimated");
        await ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, "1"));
        await ClientTest.Disconnect();
        await TakeScreenshot("07-Disconnected");
    }

    [DataTestMethod]
    [EnvironmentDataSource]
    public async Task ScrumMaster_Can_Select_Estimation_Deck(bool serverSide, bool useHttpClient)
    {
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "RPSLS";
        var scrumMaster = "Initiator";
        var deckText = "Rock, Paper, Scissors, Lizard, Spock";
        var availableEstimations = new string[]
        {
            "\uD83D\uDC8E", // Rock
            "\uD83D\uDCDC", // Paper
            "\u2702", // Scissors
            "\uD83E\uDD8E", // Lizard
            "\uD83D\uDD96", // Spock
        };

        await ClientTest.OpenApplication();
        await TakeScreenshot("01-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster, "RockPaperScissorsLizardSpock", deckText);
        await TakeScreenshot("03-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage("RPSLS", "Initiator");
        await TakeScreenshot("04-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();
        await ClientTest.StartEstimation();
        await TakeScreenshot("05-EstimationStarted");
        await ClientTest.AssertAvailableEstimations(availableEstimations);
        await ClientTest.SelectEstimation(1);
        await TakeScreenshot("06-Estimated");
        await ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, availableEstimations[1]));
        await ClientTest.Disconnect();
        await TakeScreenshot("07-Disconnected");
    }

    [DataTestMethod]
    [EnvironmentDataSource]
    public async Task Shows_Error_When_Creating_Empty_Team(bool serverSide, bool useHttpClient)
    {
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        await ClientTest.OpenApplication();
        await TakeScreenshot("01-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-Index");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("03-RequiredError");

        Assert.IsNotNull(ClientTest.CreateTeamForm);
        var input = ClientTest.CreateTeamForm.Locator(@"#createTeam\$teamName");
        var required = input.Locator("xpath=../span");
        await Assertions.Expect(required).ToHaveTextAsync("Required");

        input = ClientTest.CreateTeamForm.Locator(@"#createTeam\$scrumMasterName");
        required = input.Locator("xpath=../span");
        await Assertions.Expect(required).ToHaveTextAsync("Required");
    }

    [DataTestMethod]
    [EnvironmentDataSource]
    public async Task Shows_Error_When_Joining_Not_Existing_Team(bool serverSide, bool useHttpClient)
    {
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "My team";
        var member = "Test Member";

        await ClientTest.OpenApplication();
        await TakeScreenshot("01-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-Index");
        await ClientTest.FillJoinTeamForm(team, member);
        await TakeScreenshot("03-JoinTeamForm");
        await ClientTest.SubmitJoinTeamForm();
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("04-Error");
        await ClientTest.AssertMessageBox("Scrum Team 'My team' does not exist.");
    }

    [DataTestMethod]
    [EnvironmentDataSource]
    public async Task ScrumMaster_Can_Change_Deck(bool serverSide, bool useHttpClient)
    {
        Configure(serverSide, useHttpClient);
        await StartServer();
        await StartClients();

        var team = "My team";
        var scrumMaster = "Test ScrumMaster";
        var deckText = "0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100";
        var newDeckText = "1, 2, 3, 4, 5, 6, 7, 8, 9, 10";
        var availableEstimations = new string[]
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"
        };

        await ClientTest.OpenApplication();
        await TakeScreenshot("01-Loading");
        await ClientTest.AssertIndexPage();
        await TakeScreenshot("02-Index");
        await ClientTest.FillCreateTeamForm(team, scrumMaster, "Standard", deckText);
        await TakeScreenshot("03-CreateTeamForm");
        await ClientTest.SubmitCreateTeamForm();
        await ClientTest.AssertPlanningPokerPage("My%20team", "Test%20ScrumMaster");
        await TakeScreenshot("04-PlanningPoker");
        await ClientTest.AssertTeamName(team, scrumMaster);
        await ClientTest.AssertScrumMasterInTeam(scrumMaster);
        await ClientTest.AssertMembersInTeam();
        await ClientTest.AssertObserversInTeam();

        await ClientTest.OpenSettingsDialog();
        await TakeScreenshot("05-Settings");
        await ClientTest.AssertSettingsDialogIsOpen();
        await ClientTest.AssertSelectedDeckSetting("Standard", deckText, true);
        await ClientTest.ChangeDeck("Rating", newDeckText);
        await TakeScreenshot("06-ChangingDeck");
        await ClientTest.CloseSettingsDialog();

        await ClientTest.StartEstimation();
        await TakeScreenshot("07-EstimationStarted");
        await ClientTest.AssertAvailableEstimations(availableEstimations);

        await ClientTest.OpenSettingsDialog();
        await TakeScreenshot("08-Settings");
        await ClientTest.AssertSettingsDialogIsOpen();
        await ClientTest.AssertSelectedDeckSetting("Rating", newDeckText, false);
        await ClientTest.CloseSettingsDialog();

        await ClientTest.SelectEstimation(9);
        await TakeScreenshot("09-Estimated");
        await ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, "10"));
        await ClientTest.Disconnect();
        await TakeScreenshot("10-Disconnected");
    }
}
