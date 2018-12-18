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
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task ScrumMaster_Should_Be_Able_To_Estimate(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(ScrumMaster_Should_Be_Able_To_Estimate),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "My team";
            string scrumMaster = "Test ScrumMaster";

            ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
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
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Shows_Error_When_Creating_Empty_Team(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(Shows_Error_When_Creating_Empty_Team),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertIndexPage();
            TakeScreenshot("03-RequiredError");

            var input = ClientTest.CreateTeamForm.FindElement(By.Id("createTeam$teamName"));
            var required = input.FindElement(By.XPath("../span"));
            Assert.AreEqual("Required", required.Text);

            input = ClientTest.CreateTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));
            required = input.FindElement(By.XPath("../span"));
            Assert.AreEqual("Required", required.Text);
        }

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Shows_Error_When_Joining_Not_Existing_Team(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(Shows_Error_When_Joining_Not_Existing_Team),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "My team";
            string member = "Test Member";

            ClientTest.OpenApplication();
            TakeScreenshot("01-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-Index");
            ClientTest.FillJoinTeamForm(team, member);
            TakeScreenshot("03-JoinTeamForm");
            ClientTest.SubmitJoinTeamForm();
            await Task.Delay(500);
            ClientTest.AssertIndexPage();
            TakeScreenshot("04-Error");
            ClientTest.AssertMessageBox("Scrum Team \"My team\" does not exist.");
        }
    }
}
