using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    public class ClientTest
    {
        private static readonly string[] _availableEstimations = new string[]
            {
                "0", "\u00BD", "1", "2", "3", "5", "8", "13", "20", "40", "100", "\u221E", "?"
            };

        public ClientTest(IWebDriver browser, ServerFixture server)
        {
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public IWebDriver Browser { get; }

        public ServerFixture Server { get; }

        public IWebElement AppElement { get; private set; }

        public IWebElement PageContentElement { get; private set; }

        public IWebElement PlanningPokerContainerElement { get; private set; }

        public IWebElement ContainerElement { get; private set; }

        public IWebElement CreateTeamForm { get; private set; }

        public IWebElement JoinTeamForm { get; private set; }

        public IWebElement PlanningPokerDeskElement { get; private set; }

        public IWebElement MembersPanelElement { get; private set; }

        public Task OpenApplication()
        {
            Browser.Navigate().GoToUrl(Server.Uri);
            AppElement = Browser.FindElement(By.TagName("app"));
            Assert.IsNotNull(AppElement);
            return Task.Delay(2000);
        }

        public void AssertIndexPage()
        {
            PageContentElement = AppElement.FindElement(By.CssSelector("div.pageContent"));
            PlanningPokerContainerElement = PageContentElement.FindElement(By.CssSelector("div.planningPokerContainer"));
            ContainerElement = PlanningPokerContainerElement.FindElement(By.XPath("./div[@class='container']"));
            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            JoinTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='joinTeam']"));
        }

        public void FillCreateTeamForm(string team, string scrumMaster)
        {
            var teamNameInput = CreateTeamForm.FindElement(By.Id("createTeam$teamName"));
            var scrumMasterNameInput = CreateTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));

            teamNameInput.SendKeys(team);
            scrumMasterNameInput.SendKeys(scrumMaster);

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, scrumMasterNameInput.FindElements(By.XPath("../span")).Count);
        }

        public void SubmitCreateTeamForm()
        {
            var submitButton = CreateTeamForm.FindElement(By.Id("createTeam$Submit"));
            submitButton.Click();
        }

        public void FillJoinTeamForm(string team, string member)
        {
            FillJoinTeamForm(team, member, false);
        }

        public void FillJoinTeamForm(string team, string member, bool asObserver)
        {
            var teamNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$teamName"));
            var memberNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$memberName"));
            var observerInput = JoinTeamForm.FindElement(By.Id("joinTeam$asObserver"));

            teamNameInput.SendKeys(team);
            memberNameInput.SendKeys(member);
            if (asObserver)
            {
                observerInput.Click();
            }

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, memberNameInput.FindElements(By.XPath("../span")).Count);
        }

        public void SubmitJoinTeamForm()
        {
            var submitButton = JoinTeamForm.FindElement(By.Id("joinTeam$submit"));
            submitButton.Click();
        }

        public void AssertPlanningPokerPage(string team, string scrumMaster)
        {
            PlanningPokerDeskElement = ContainerElement.FindElement(By.CssSelector("div.pokerDeskPanel"));
            MembersPanelElement = ContainerElement.FindElement(By.CssSelector("div.membersPanel"));

            Assert.AreEqual($"{Server.Uri}PlanningPoker/{team}/{scrumMaster}", Browser.Url);
        }

        public void AssertTeamName(string team, string member)
        {
            var teamNameHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h2"));
            Assert.AreEqual(team, teamNameHeader.Text);
            var userHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h3"));
            Assert.AreEqual(member, userHeader.Text);
        }

        public void AssertScrumMasterInTeam(string scrumMaster)
        {
            var scrumMasterElements = MembersPanelElement.FindElements(By.XPath("./div/ul[1]/li/span[1]"));
            Assert.AreEqual(1, scrumMasterElements.Count);
            Assert.AreEqual(scrumMaster, scrumMasterElements[0].Text);
        }

        public void AssertMembersInTeam(params string[] members)
        {
            var elements = MembersPanelElement.FindElements(By.XPath("./div/ul[2]/li/span[1]"));
            if (members == null)
            {
                Assert.AreEqual(0, elements.Count);
            }
            else
            {
                Assert.AreEqual(members.Length, elements.Count);
                CollectionAssert.AreEqual(members, elements.Select(e => e.Text).ToList());
            }
        }

        public void AssertObserversInTeam(params string[] observers)
        {
            var elements = MembersPanelElement.FindElements(By.XPath("./div/ul[3]/li/span"));
            if (observers == null)
            {
                Assert.AreEqual(0, elements.Count);
            }
            else
            {
                Assert.AreEqual(observers.Length, elements.Count);
                CollectionAssert.AreEqual(observers, elements.Select(e => e.Text).ToList());
            }
        }

        public void StartEstimation()
        {
            var button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar button"));
            Assert.AreEqual("Start estimation", button.Text);

            button.Click();

            PlanningPokerDeskElement.FindElement(By.CssSelector("div.availableEstimations"));
        }

        public void CancelEstimation()
        {
            var button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar button"));
            Assert.AreEqual("Cancel estimation", button.Text);
            button.Click();
        }

        public void AssertAvailableEstimations()
        {
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations ul li a"));
            Assert.AreEqual(13, availableEstimationElements.Count);
            CollectionAssert.AreEqual(_availableEstimations, availableEstimationElements.Select(e => e.Text).ToList());
        }

        public void AssertNotAvailableEstimations()
        {
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations"));
            Assert.AreEqual(0, availableEstimationElements.Count);
        }

        public void SelectEstimation(string estimation)
        {
            int index = Array.IndexOf<string>(_availableEstimations, estimation);
            var availableEstimationElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimations ul li a"));
            availableEstimationElements[index].Click();
        }

        public void AssertSelectedEstimation(params KeyValuePair<string, string>[] estimations)
        {
            var estimationResultElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.estimationResult ul li"));
            if (estimations == null)
            {
                Assert.AreEqual(0, estimationResultElements.Count);
            }
            else
            {
                Assert.AreEqual(estimations.Length, estimationResultElements.Count);

                for (int i = 0; i < estimations.Length; i++)
                {
                    var estimation = estimations[i];
                    var estimationResultElement = estimationResultElements[i];
                    var valueElement = estimationResultElement.FindElement(By.XPath("./span[1]"));
                    var nameElement = estimationResultElement.FindElement(By.XPath("./span[2]"));
                    Assert.AreEqual(estimation.Key, nameElement.Text);
                    Assert.AreEqual(estimation.Value, valueElement.Text);
                }
            }
        }

        public void Disconnect()
        {
            var navbarPlanningPokerElement = PlanningPokerContainerElement.FindElement(By.TagName("nav"));
            var disconnectElement = navbarPlanningPokerElement.FindElement(By.CssSelector("ul a"));
            Assert.AreEqual("Disconnect", disconnectElement.Text);

            disconnectElement.Click();

            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            Assert.AreEqual($"{Server.Uri}Index", Browser.Url);
        }

        public void AssertMessageBox(string text)
        {
            var messageBoxElement = PageContentElement.FindElement(By.Id("messageBox"));
            Assert.AreEqual("block", messageBoxElement.GetCssValue("display"));

            var messageBodyElement = messageBoxElement.FindElement(By.CssSelector("div.modal-body"));
            Assert.AreEqual(text, messageBodyElement.Text);
        }
    }
}
