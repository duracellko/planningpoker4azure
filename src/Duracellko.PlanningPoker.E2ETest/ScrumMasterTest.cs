using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    [TestClass]
    public class ScrumMasterTest : E2ETestBase
    {
        private IWebElement _appElement;
        private IWebElement _planningPokerContainerElement;
        private IWebElement _containerElement;
        private IWebElement _createTeamForm;
        private IWebElement _planningPokerDeskElement;
        private IWebElement _membersPanelElement;

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task ScrumMaster_Should_Be_Able_To_Estimate(bool serverSide, BrowserType browserType)
        {
            Context = new BrowserTestContext(
                nameof(ScrumMasterTest),
                nameof(ScrumMaster_Should_Be_Able_To_Estimate),
                browserType,
                serverSide);

            Server.UseServerSide = serverSide;
            await Server.Start();
            await AssertServerSide(serverSide);

            BrowserFixture.Initialize(Context.BrowserType);

            LoadApplication();
            OpenIndexPage();
            FillInCreateTeamForm();
            SubmitCreateTeamForm();
            AssertScrumMasterInTeam();
            AssertTeamName();
            StartEstimation();
            await SelectEstimation();
            Disconnect();
        }

        private void LoadApplication()
        {
            Browser.Navigate().GoToUrl(Server.Uri);
            _appElement = Browser.FindElement(By.TagName("app"));
            Assert.IsNotNull(_appElement);
            TakeScreenshot("01-Loading");
        }

        private void OpenIndexPage()
        {
            _planningPokerContainerElement = _appElement.FindElement(By.CssSelector("div.planningPokerContainer"));
            _containerElement = _planningPokerContainerElement.FindElement(By.XPath("./div[@class='container']"));
            TakeScreenshot("02-Index");
        }

        private void FillInCreateTeamForm()
        {
            _createTeamForm = _containerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            var teamNameInput = _createTeamForm.FindElement(By.Id("createTeam$teamName"));
            var scrumMasterNameInput = _createTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));

            teamNameInput.SendKeys("My team");
            scrumMasterNameInput.SendKeys("Test ScrumMaster");

            TakeScreenshot("03-CreateTeamForm");

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, scrumMasterNameInput.FindElements(By.XPath("../span")).Count);
        }

        private void SubmitCreateTeamForm()
        {
            var submitButton = _createTeamForm.FindElement(By.Id("createTeam$Submit"));
            submitButton.Click();

            _planningPokerDeskElement = _containerElement.FindElement(By.CssSelector("div.pokerDeskPanel"));
            _membersPanelElement = _containerElement.FindElement(By.CssSelector("div.membersPanel"));
            TakeScreenshot("04-PlanningPoker");

            Assert.AreEqual($"{Server.Uri}PlanningPoker/My%20team/Test%20ScrumMaster", Browser.Url);
        }

        private void AssertScrumMasterInTeam()
        {
            var scrumMasterElements = _membersPanelElement.FindElements(By.XPath("./div[1]/ul/li"));
            Assert.AreEqual(1, scrumMasterElements.Count);
            Assert.AreEqual("Test ScrumMaster", scrumMasterElements[0].Text);

            var elements = _membersPanelElement.FindElements(By.XPath("./div[2]/ul/li"));
            Assert.AreEqual(0, elements.Count);
            elements = _membersPanelElement.FindElements(By.XPath("./div[3]/ul/li"));
            Assert.AreEqual(0, elements.Count);
        }

        private void AssertTeamName()
        {
            var teamNameHeader = _planningPokerDeskElement.FindElement(By.CssSelector("div.team-title h2"));
            Assert.AreEqual("My team", teamNameHeader.Text);
            var userHeader = _planningPokerDeskElement.FindElement(By.CssSelector("div.team-title h3"));
            Assert.AreEqual("Test ScrumMaster", userHeader.Text);
        }

        private void StartEstimation()
        {
            var button = _planningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar a"));
            Assert.AreEqual("Start estimation", button.Text);

            button.Click();

            _planningPokerDeskElement.FindElement(By.CssSelector("div.availableEstimations"));
            TakeScreenshot("05-EstimationStarted");
        }

        private async Task SelectEstimation()
        {
            var availableEstimationElements = _planningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations ul li a"));
            Assert.AreEqual(13, availableEstimationElements.Count);
            Assert.AreEqual("0", availableEstimationElements[0].Text);
            Assert.AreEqual("\u00BD", availableEstimationElements[1].Text);
            Assert.AreEqual("1", availableEstimationElements[2].Text);
            Assert.AreEqual("2", availableEstimationElements[3].Text);
            Assert.AreEqual("3", availableEstimationElements[4].Text);
            Assert.AreEqual("5", availableEstimationElements[5].Text);
            Assert.AreEqual("8", availableEstimationElements[6].Text);
            Assert.AreEqual("13", availableEstimationElements[7].Text);
            Assert.AreEqual("20", availableEstimationElements[8].Text);
            Assert.AreEqual("40", availableEstimationElements[9].Text);
            Assert.AreEqual("100", availableEstimationElements[10].Text);
            Assert.AreEqual("\u221E", availableEstimationElements[11].Text);
            Assert.AreEqual("?", availableEstimationElements[12].Text);

            availableEstimationElements[2].Click();

            await Task.Delay(500);
            TakeScreenshot("06-Estimated");
        }

        private void Disconnect()
        {
            var navbarPlanningPokerElement = _planningPokerContainerElement.FindElement(By.Id("navbarPlanningPoker"));
            var disconnectElement = navbarPlanningPokerElement.FindElement(By.TagName("a"));
            Assert.AreEqual("Disconnect", disconnectElement.Text);

            disconnectElement.Click();

            _createTeamForm = _containerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            TakeScreenshot("07-Disconnected");
            Assert.AreEqual($"{Server.Uri}Index", Browser.Url);
        }
    }
}
