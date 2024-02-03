using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest
{
    public class ClientTest
    {
        private static readonly string[] _availableEstimations = new string[]
        {
            "0", "\u00BD", "1", "2", "3", "5", "8", "13", "20", "40", "100", "\u221E", "?"
        };

        public ClientTest(IPage page, Uri serverUri)
        {
            Page = page ?? throw new ArgumentNullException(nameof(page));
            ServerUri = serverUri ?? throw new ArgumentNullException(nameof(serverUri));
        }

        public IPage Page { get; }

        public Uri ServerUri { get; }

        public ILocator? AppElement { get; private set; }

        public ILocator? PageContentElement { get; private set; }

        public ILocator? PlanningPokerContainerElement { get; private set; }

        public ILocator? ContainerElement { get; private set; }

        public ILocator? CreateTeamForm { get; private set; }

        public ILocator? JoinTeamForm { get; private set; }

        public ILocator? PlanningPokerDeskElement { get; private set; }

        public ILocator? MembersPanelElement { get; private set; }

        public ILocator? SettingsDialogElement { get; private set; }

        public async Task OpenApplication()
        {
            await Page.GotoAsync(ServerUri.ToString());
            AppElement = Page.Locator("#app");
            await Assertions.Expect(AppElement).ToBeVisibleAsync();
            await Task.Delay(2000);
        }

        public async Task AssertIndexPage()
        {
            Assert.IsNotNull(AppElement);
            PageContentElement = AppElement.Locator("div.pageContent");
            PlanningPokerContainerElement = PageContentElement.Locator("div.planningPokerContainer");
            ContainerElement = PlanningPokerContainerElement.Locator("xpath=./div[@class='container']");
            CreateTeamForm = ContainerElement.Locator("form[name='createTeam']");
            JoinTeamForm = ContainerElement.Locator("form[name='joinTeam']");
            await Assertions.Expect(CreateTeamForm).ToBeVisibleAsync();
            await Assertions.Expect(JoinTeamForm).ToBeVisibleAsync();
        }

        public async Task FillCreateTeamForm(string team, string scrumMaster, string? deck = null, string? deckText = null)
        {
            Assert.IsNotNull(CreateTeamForm);
            var teamNameInput = CreateTeamForm.Locator(@"#createTeam\$teamName");
            var scrumMasterNameInput = CreateTeamForm.Locator(@"#createTeam\$scrumMasterName");
            var selectDeckInput = CreateTeamForm.Locator(@"#createTeam\$selectedDeck");

            await teamNameInput.FillAsync(team);
            await scrumMasterNameInput.FillAsync(scrumMaster);

            Assert.AreEqual(0, await teamNameInput.Locator("xpath=../span").CountAsync());
            Assert.AreEqual(0, await scrumMasterNameInput.Locator("xpath=../span").CountAsync());

            if (deck != null)
            {
                await selectDeckInput.SelectOptionAsync(deck);
                await Assertions.Expect(selectDeckInput).ToHaveValueAsync(deck);

                if (deckText != null)
                {
                    var selectedOption = selectDeckInput.Locator($"option[value=\"{deck}\"]");
                    await Assertions.Expect(selectedOption).ToHaveTextAsync(deckText);
                }
            }
        }

        public async Task SubmitCreateTeamForm()
        {
            Assert.IsNotNull(CreateTeamForm);
            var submitButton = CreateTeamForm.Locator(@"#createTeam\$Submit");
            await submitButton.ClickAsync();
        }

        public Task FillJoinTeamForm(string team, string member)
        {
            return FillJoinTeamForm(team, member, false);
        }

        public async Task FillJoinTeamForm(string team, string member, bool asObserver)
        {
            Assert.IsNotNull(JoinTeamForm);
            var teamNameInput = JoinTeamForm.Locator(@"#joinTeam\$teamName");
            var memberNameInput = JoinTeamForm.Locator(@"#joinTeam\$memberName");
            var observerInput = JoinTeamForm.Locator(@"#joinTeam\$asObserver");

            await teamNameInput.FillAsync(team);
            await memberNameInput.FillAsync(member);
            if (asObserver)
            {
                await observerInput.CheckAsync();
            }

            Assert.AreEqual(0, await teamNameInput.Locator("xpath=../span").CountAsync());
            Assert.AreEqual(0, await memberNameInput.Locator("xpath=../span").CountAsync());
        }

        public async Task SubmitJoinTeamForm()
        {
            Assert.IsNotNull(JoinTeamForm);
            var submitButton = JoinTeamForm.Locator(@"#joinTeam\$submit");
            await submitButton.ClickAsync();
        }

        public async Task AssertPlanningPokerPage(string team, string scrumMaster)
        {
            Assert.IsNotNull(ContainerElement);
            PlanningPokerDeskElement = ContainerElement.Locator("div.pokerDeskPanel");
            MembersPanelElement = ContainerElement.Locator("div.membersPanel");

            await Assertions.Expect(PlanningPokerDeskElement).ToBeVisibleAsync();
            await Assertions.Expect(MembersPanelElement).ToBeVisibleAsync();
            await Assertions.Expect(Page).ToHaveURLAsync($"{ServerUri}PlanningPoker/{team}/{scrumMaster}");
        }

        public async Task AssertTeamName(string team, string member)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var teamNameHeader = PlanningPokerDeskElement.Locator("div.team-title h2");
            await Assertions.Expect(teamNameHeader).ToHaveTextAsync(team);
            var userHeader = PlanningPokerDeskElement.Locator("div.team-title h3");
            await Assertions.Expect(userHeader).ToHaveTextAsync(member);
        }

        public async Task AssertScrumMasterInTeam(string scrumMaster)
        {
            Assert.IsNotNull(MembersPanelElement);
            var scrumMasterElements = MembersPanelElement.Locator("xpath=./div/ul[1]/li/span[1]");
            Assert.AreEqual(1, await scrumMasterElements.CountAsync());
            await Assertions.Expect(scrumMasterElements.First).ToHaveTextAsync(scrumMaster);
        }

        public async Task AssertMembersInTeam(params string[] members)
        {
            ArgumentNullException.ThrowIfNull(members);

            Assert.IsNotNull(MembersPanelElement);
            var elements = MembersPanelElement.Locator("xpath=./div/ul[2]/li/span[1]");
            await AssertElementsText(elements, members);
        }

        public async Task AssertObserversInTeam(params string[] observers)
        {
            ArgumentNullException.ThrowIfNull(observers);

            Assert.IsNotNull(MembersPanelElement);
            var elements = MembersPanelElement.Locator("xpath=./div/ul[3]/li/span");
            await AssertElementsText(elements, observers);
        }

        public async Task StartEstimation()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var button = PlanningPokerDeskElement.Locator("div.actionsBar button").First;
            await Assertions.Expect(button).ToHaveTextAsync("Start estimation");

            await button.ClickAsync();

            await Assertions.Expect(PlanningPokerDeskElement.Locator("div.availableEstimations")).ToBeVisibleAsync();
        }

        public async Task CancelEstimation()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var button = PlanningPokerDeskElement.Locator("div.actionsBar button").First;
            await Assertions.Expect(button).ToHaveTextAsync("Cancel estimation");
            await button.ClickAsync();
        }

        public async Task ShowAverage(bool isScrumMaster)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var buttons = PlanningPokerDeskElement.Locator("div.actionsBar button");
            Assert.AreEqual(isScrumMaster ? 3 : 2, await buttons.CountAsync());

            var button = buttons.Nth(isScrumMaster ? 1 : 0);
            await Assertions.Expect(button).ToHaveTextAsync("Show average");
            await button.ClickAsync();
        }

        public async Task AssertAvailableEstimations(ICollection<string>? estimations = null)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.Locator("div.availableEstimations ul li a");
            var expectedEstimations = estimations?.ToArray() ?? _availableEstimations;
            await AssertElementsText(availableEstimationElements, expectedEstimations);
        }

        public async Task AssertNotAvailableEstimations()
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.Locator("div.availableEstimations");
            Assert.AreEqual(0, await availableEstimationElements.CountAsync());
        }

        public Task SelectEstimation(string estimation)
        {
            int index = Array.IndexOf<string>(_availableEstimations, estimation);
            return SelectEstimation(index);
        }

        public async Task SelectEstimation(int index)
        {
            Assert.IsNotNull(PlanningPokerDeskElement);
            var availableEstimationElements = PlanningPokerDeskElement.Locator("div.availableEstimations ul li a");
            await availableEstimationElements.Nth(index).ClickAsync();
        }

        public async Task AssertSelectedEstimation(params KeyValuePair<string, string>[] estimations)
        {
            ArgumentNullException.ThrowIfNull(estimations);

            Assert.IsNotNull(PlanningPokerDeskElement);
            var estimationResultElements = PlanningPokerDeskElement.Locator("div.estimationResult ul li");
            Assert.AreEqual(estimations.Length, await estimationResultElements.CountAsync());

            for (int i = 0; i < estimations.Length; i++)
            {
                var estimation = estimations[i];
                var estimationResultElement = estimationResultElements.Nth(i);
                var valueElement = estimationResultElement.Locator("xpath=./span[1]");
                var nameElement = estimationResultElement.Locator("xpath=./span[2]");
                await Assertions.Expect(nameElement).ToHaveTextAsync(estimation.Key);
                await Assertions.Expect(valueElement).ToHaveTextAsync(estimation.Value);
            }
        }

        public async Task AssertEstimationSummary(double average, double median, double sum)
        {
            var summaryItems = new[]
            {
                KeyValuePair.Create("Average", average),
                KeyValuePair.Create("Median", median),
                KeyValuePair.Create("Sum", sum),
            };

            Assert.IsNotNull(PlanningPokerDeskElement);
            var summaryItemElements = PlanningPokerDeskElement.Locator("div.estimationResult div.estimationSummary div.card");
            Assert.AreEqual(summaryItems.Length, await summaryItemElements.CountAsync());

            for (int i = 0; i < summaryItems.Length; i++)
            {
                var summaryItem = summaryItems[i];
                var summaryItemElement = summaryItemElements.Nth(i);
                var nameElement = summaryItemElement.Locator("div.card-header");
                var valueElement = summaryItemElement.Locator("div.card-body");
                await Assertions.Expect(nameElement).ToHaveTextAsync(summaryItem.Key);
                await Assertions.Expect(valueElement).ToHaveTextAsync(summaryItem.Value.ToString("N2", CultureInfo.InvariantCulture));
            }
        }

        public async Task OpenSettingsDialog()
        {
            Assert.IsNotNull(PlanningPokerContainerElement);
            var navbarPlanningPokerElement = PlanningPokerContainerElement.Locator("nav");
            var settingsElement = navbarPlanningPokerElement.Locator("ul li.nav-item:first-child a.nav-link");
            await Assertions.Expect(settingsElement).ToHaveTextAsync("Settings");

            await settingsElement.ClickAsync();

            SettingsDialogElement = PlanningPokerContainerElement.Locator("#planningPokerSettingsModal");
        }

        public async Task AssertSettingsDialogIsOpen()
        {
            Assert.IsNotNull(SettingsDialogElement);
            await Assertions.Expect(SettingsDialogElement).ToBeVisibleAsync();
            await Assertions.Expect(SettingsDialogElement).ToHaveCSSAsync("display", "block");

            var modalContentElement = SettingsDialogElement.Locator("div.modal-dialog div.modal-content");
            await Assertions.Expect(modalContentElement).ToBeVisibleAsync();
            await Assertions.Expect(modalContentElement).ToHaveCSSAsync("display", "flex");

            var modalTitleElement = modalContentElement.Locator("div.modal-header h5.modal-title");
            await Assertions.Expect(modalTitleElement).ToHaveTextAsync("Settings");
        }

        public async Task AssertSelectedDeckSetting(string deck, string deckText, bool isEnabled)
        {
            Assert.IsNotNull(SettingsDialogElement);
            var modalBodyElement = SettingsDialogElement.Locator("div.modal-dialog div.modal-content div.modal-body");
            var formElement = modalBodyElement.Locator("form");
            var selectedDeckInput = formElement.Locator(@"#planningPokerSettings\$selectedDeck");

            await Assertions.Expect(selectedDeckInput).ToHaveValueAsync(deck);
            await Assertions.Expect(selectedDeckInput).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions() { Enabled = isEnabled });
            var selectedOption = selectedDeckInput.Locator($"option[value=\"{deck}\"]");
            await Assertions.Expect(selectedOption).ToHaveTextAsync(deckText);

            var changeDeckButton = formElement.Locator(@"#planningPokerSettings\$changeDeckButton");
            await Assertions.Expect(changeDeckButton).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions() { Enabled = isEnabled });
        }

        public async Task ChangeDeck(string deck, string deckText)
        {
            Assert.IsNotNull(SettingsDialogElement);
            var modalBodyElement = SettingsDialogElement.Locator("div.modal-dialog div.modal-content div.modal-body");
            var formElement = modalBodyElement.Locator("form");
            var selectedDeckInput = formElement.Locator(@"#planningPokerSettings\$selectedDeck");
            var changeDeckButton = formElement.Locator(@"#planningPokerSettings\$changeDeckButton");

            await selectedDeckInput.SelectOptionAsync(deck);

            var selectedOption = selectedDeckInput.Locator($"option[value=\"{deck}\"]");
            await Assertions.Expect(selectedOption).ToHaveTextAsync(deckText);

            await changeDeckButton.ClickAsync();
        }

        public async Task CloseSettingsDialog()
        {
            Assert.IsNotNull(SettingsDialogElement);
            var modalContentElement = SettingsDialogElement.Locator("div.modal-dialog div.modal-content");
            var closeModalButton = modalContentElement.Locator("div.modal-header button.btn-close");
            await closeModalButton.ClickAsync();
        }

        public async Task Disconnect()
        {
            Assert.IsNotNull(PlanningPokerContainerElement);
            var navbarPlanningPokerElement = PlanningPokerContainerElement.Locator("nav");
            var disconnectElement = navbarPlanningPokerElement.Locator("ul li.nav-item:last-child a.nav-link");
            await Assertions.Expect(disconnectElement).ToHaveTextAsync("Disconnect");

            await disconnectElement.ClickAsync();

            Assert.IsNotNull(ContainerElement);
            CreateTeamForm = ContainerElement.Locator("form[name='createTeam']");
            await Assertions.Expect(Page).ToHaveURLAsync($"{ServerUri}Index");
        }

        public async Task AssertMessageBox(string text)
        {
            Assert.IsNotNull(PageContentElement);
            var messageBoxElement = PageContentElement.Locator("#messageBox");
            await Assertions.Expect(messageBoxElement).ToBeVisibleAsync();
            await Assertions.Expect(messageBoxElement).ToHaveCSSAsync("display", "block");

            var messageBodyElement = messageBoxElement.Locator("div.modal-body");
            await Assertions.Expect(messageBodyElement).ToHaveTextAsync(text);
        }

        private static async Task AssertElementsText(ILocator elements, string[] texts)
        {
            Assert.AreEqual(texts.Length, await elements.CountAsync());
            for (int i = 0; i < texts.Length; i++)
            {
                await Assertions.Expect(elements.Nth(i)).ToHaveTextAsync(texts[i]);
            }
        }
    }
}
