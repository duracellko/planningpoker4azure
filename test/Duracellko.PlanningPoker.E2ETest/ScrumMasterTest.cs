using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    [TestClass]
    public class ScrumMasterTest : E2ETestBase
    {
        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task ScrumMaster_Should_Be_Able_To_Estimate(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(ScrumMaster_Should_Be_Able_To_Estimate),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "My team";
            string scrumMaster = "Test ScrumMaster";
            string deckText = "0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100";

            await ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster, "Standard", deckText);
            TakeScreenshot("03-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage("My%20team", "Test%20ScrumMaster");
            TakeScreenshot("04-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();
            ClientTest.StartEstimation();
            TakeScreenshot("05-EstimationStarted");
            ClientTest.AssertAvailableEstimations();
            ClientTest.SelectEstimation("1");
            await Task.Delay(500);
            TakeScreenshot("06-Estimated");
            ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, "1"));
            ClientTest.Disconnect();
            TakeScreenshot("07-Disconnected");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task ScrumMaster_Can_Select_Estimation_Deck(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(ScrumMaster_Can_Select_Estimation_Deck),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "RPSLS";
            string scrumMaster = "Initiator";
            string deckText = "Rock, Paper, Scissors, Lizard, Spock";
            var availableEstimations = new string[]
            {
                "\uD83D\uDC8E", // Rock
                "\uD83D\uDCDC", // Paper
                "\u2702", // Scissors
                "\uD83E\uDD8E", // Lizard
                "\uD83D\uDD96", // Spock
            };

            await ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster, "RockPaperScissorsLizardSpock", deckText);
            TakeScreenshot("03-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage("RPSLS", "Initiator");
            TakeScreenshot("04-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();
            ClientTest.StartEstimation();
            TakeScreenshot("05-EstimationStarted");
            ClientTest.AssertAvailableEstimations(availableEstimations);
            ClientTest.SelectEstimation(1);
            await Task.Delay(500);
            TakeScreenshot("06-Estimated");
            ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, availableEstimations[1]));
            ClientTest.Disconnect();
            TakeScreenshot("07-Disconnected");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Shows_Error_When_Creating_Empty_Team(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(Shows_Error_When_Creating_Empty_Team),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            await ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertIndexPage();
            TakeScreenshot("03-RequiredError");

            Assert.IsNotNull(ClientTest.CreateTeamForm);
            var input = ClientTest.CreateTeamForm.FindElement(By.Id("createTeam$teamName"));
            var required = input.FindElement(By.XPath("../span"));
            Assert.AreEqual("Required", required.Text);

            input = ClientTest.CreateTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));
            required = input.FindElement(By.XPath("../span"));
            Assert.AreEqual("Required", required.Text);
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task Shows_Error_When_Joining_Not_Existing_Team(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(Shows_Error_When_Joining_Not_Existing_Team),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "My team";
            string member = "Test Member";

            await ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillJoinTeamForm(team, member);
            TakeScreenshot("03-JoinTeamForm");
            ClientTest.SubmitJoinTeamForm();
            await Task.Delay(500);
            ClientTest.AssertIndexPage();
            TakeScreenshot("04-Error");
            ClientTest.AssertMessageBox("Scrum Team \"My team\" does not exist. (Parameter 'teamName')");
        }

        [DataTestMethod]
        [EnvironmentDataSource]
        public async Task ScrumMaster_Can_Change_Deck(bool serverSide, BrowserType browserType, bool useHttpClient)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(ScrumMaster_Can_Change_Deck),
                browserType,
                serverSide,
                useHttpClient));

            await StartServer();
            StartClients();

            string team = "My team";
            string scrumMaster = "Test ScrumMaster";
            string deckText = "0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100";
            string newDeckText = "1, 2, 3, 4, 5, 6, 7, 8, 9, 10";
            var availableEstimations = new string[]
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"
            };

            await ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster, "Standard", deckText);
            TakeScreenshot("03-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage("My%20team", "Test%20ScrumMaster");
            TakeScreenshot("04-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            ClientTest.OpenSettingsDialog();
            await Task.Delay(200);
            TakeScreenshot("05-Settings");
            ClientTest.AssertSettingsDialogIsOpen();
            ClientTest.AssertSelectedDeckSetting(deckText, true);
            ClientTest.ChangeDeck("Rating", newDeckText);
            TakeScreenshot("06-ChangingDeck");
            await Task.Delay(500);
            ClientTest.CloseSettingsDialog();
            await Task.Delay(200);

            ClientTest.StartEstimation();
            TakeScreenshot("07-EstimationStarted");
            ClientTest.AssertAvailableEstimations(availableEstimations);

            ClientTest.OpenSettingsDialog();
            await Task.Delay(200);
            TakeScreenshot("08-Settings");
            ClientTest.AssertSettingsDialogIsOpen();
            ClientTest.AssertSelectedDeckSetting(newDeckText, false);
            ClientTest.CloseSettingsDialog();
            await Task.Delay(200);

            ClientTest.SelectEstimation(9);
            await Task.Delay(500);
            TakeScreenshot("09-Estimated");
            ClientTest.AssertSelectedEstimation(new KeyValuePair<string, string>(scrumMaster, "10"));
            ClientTest.Disconnect();
            TakeScreenshot("10-Disconnected");
        }
    }
}
