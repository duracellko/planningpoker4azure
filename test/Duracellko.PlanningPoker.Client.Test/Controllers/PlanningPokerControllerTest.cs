using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerTest
    {
        private CultureInfo? _originalCultureInfo;

        public static IEnumerable<object[]> TimerStartedTestData { get; } = new[]
        {
            new object[] { new DateTime(2021, 11, 18, 10, 21, 5, DateTimeKind.Utc), TimeSpan.Zero, TimeSpan.FromSeconds(2) },
            new object[] { new DateTime(2021, 11, 18, 10, 22, 45, DateTimeKind.Utc), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(87) },
            new object[] { new DateTime(2021, 11, 18, 10, 20, 45, DateTimeKind.Utc), TimeSpan.FromSeconds(-55), TimeSpan.FromSeconds(37) },
            new object[] { new DateTime(2021, 11, 18, 10, 30, 19, DateTimeKind.Utc), TimeSpan.FromSeconds(-91), TimeSpan.FromSeconds(647) }
        };

        public static IEnumerable<object[]> TimerFinishedTestData { get; } = new[]
        {
            new object[] { new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc), TimeSpan.Zero },
            new object[] { new DateTime(2021, 11, 18, 10, 21, 45, DateTimeKind.Utc), TimeSpan.FromSeconds(55) },
            new object[] { new DateTime(2021, 11, 18, 10, 21, 1, DateTimeKind.Utc), TimeSpan.FromSeconds(-1) },
            new object[] { new DateTime(2021, 11, 18, 10, 18, 21, DateTimeKind.Utc), TimeSpan.FromSeconds(-21) }
        };

        [TestInitialize]
        public void TestInitialize()
        {
            _originalCultureInfo = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_originalCultureInfo != null)
            {
                CultureInfo.CurrentCulture = _originalCultureInfo;
                _originalCultureInfo = null;
            }
        }

        [TestMethod]
        public void Constructor_IsConnected_False()
        {
            using var target = CreateController();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public void EstimationDecks_Get_ReturnsEstimationDecks()
        {
            using var target = CreateController();

            var result = target.EstimationDecks;

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100", result[Deck.Standard]);
            Assert.AreEqual("0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89", result[Deck.Fibonacci]);
            Assert.AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9, 10", result[Deck.Rating]);
            Assert.AreEqual("T-shirt: XS, S, M, L, XL", result[Deck.Tshirt]);
            Assert.AreEqual("Rock, Paper, Scissors, Lizard, Spock", result[Deck.RockPaperScissorsLizardSpock]);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_TeamNameIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(sessionId, target.SessionId);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.IsNotNull(target.User);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberName_IsNotScrumMaster()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(sessionId, target.SessionId);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.IsNotNull(target.User);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberNameIsLowerCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, "test member", null);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(sessionId, target.SessionId);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.IsNotNull(target.User);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterNameIsUpperCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, "TEST SCRUM MASTER", null);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(sessionId, target.SessionId);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.IsNotNull(target.User);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_ObserverName_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ObserverName, null);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(sessionId, target.SessionId);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.IsNotNull(target.User);
            Assert.AreEqual(PlanningPokerData.ObserverName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ObserverType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_LastMessageId_IsMinus1()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            var sessionId = teamResult.SessionId;
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(-1, target.LastMessageId);
            Assert.AreEqual(sessionId, target.SessionId);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_ScrumMasterIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsNotNull(target.ScrumMaster);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster.Name);
            Assert.IsFalse(target.ScrumMaster.HasEstimated);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.AreEqual(1, target.Members.Count());
            Assert.AreEqual(PlanningPokerData.MemberName, target.Members.First().Name);
            Assert.IsFalse(target.Members.First().HasEstimated);
            Assert.AreEqual(1, target.Observers.Count());
            Assert.AreEqual(PlanningPokerData.ObserverName, target.Observers.First().Name);
            Assert.IsFalse(target.Observers.First().HasEstimated);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWith4MembersAnd3Observers_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Name = "me", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "1st Member", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "XYZ", Type = PlanningPokerData.MemberType });
            scrumTeam.Observers.Add(new TeamMember { Name = "ABC", Type = PlanningPokerData.ObserverType });
            scrumTeam.Observers.Add(new TeamMember { Name = "Hello, World!", Type = PlanningPokerData.ObserverType });
            var teamResult = CreateTeamResult(scrumTeam);

            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            var expectedMembers = new string[] { "1st Member", "me", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            var expectedObservers = new string[] { "ABC", "Hello, World!", PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.Select(m => m.Name).ToList());

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWithMembersSetToNull_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members = null!;
            scrumTeam.Observers = null!;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            CollectionAssert.AreEqual(Array.Empty<string>(), target.Members.ToList());
            CollectionAssert.AreEqual(Array.Empty<string>(), target.Observers.ToList());
            Assert.IsNotNull(target.ScrumTeam?.Members);
            Assert.AreEqual(0, target.ScrumTeam.Members.Count);
            Assert.IsNotNull(target.ScrumTeam.Observers);
            Assert.AreEqual(0, target.ScrumTeam.Observers.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_AvailableEstimationsAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            var expectedEstimations = new double?[] { 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100, double.PositiveInfinity, null };
            CollectionAssert.AreEqual(expectedEstimations, target.AvailableEstimations.ToList());
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndInitialState_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationInProgress_CanCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationFinished_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationCanceled_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndInitialState_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationInProgress_CannotCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationFinished_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationCanceled_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAnd5Estimations_5Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = 3 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = 2 }
                }
            };
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(5, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual("Tester", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(2.0, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(3.0, estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndMemberWithoutEstimation_4Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                }
            };

            var sessionId = Guid.NewGuid();
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = sessionId,
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = double.PositiveInfinity }
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(sessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(5, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Tester", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndSameEstimationCount_6Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = 20 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                    Estimation = new Estimation { Value = 13 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = 13 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                    Estimation = new Estimation { Value = 20 }
                }
            };

            var sessionId = Guid.NewGuid();
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = sessionId,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(sessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(6, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(13.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Tester 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(13.0, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(20.0, estimation.Estimation);

            estimation = estimations[5];
            Assert.AreEqual("Tester 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(20.0, estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndSameEstimationCountWithNull_6Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 5 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                    Estimation = new Estimation { Value = 5 }
                }
            };
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(6, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(5.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Tester 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(5.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[5];
            Assert.AreEqual("Tester 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndEmptyEstimationsList_NoEstimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(teamResult.SessionId, target.SessionId);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            Assert.AreEqual(0, target.Estimations.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAnd4Participants_3Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimationParticipantStatus { MemberName = "Tester", Estimated = false },
                new EstimationParticipantStatus { MemberName = "Developer 1", Estimated = true },
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = 8 }
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoObserverHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(3, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            Assert.IsNotNull(target.ScrumMaster);
            Assert.IsTrue(target.ScrumMaster.HasEstimated);
            AssertMemberHasEstimated(target, "Tester", false);
            AssertMemberHasEstimated(target, "Developer 1", true);
            AssertMemberHasEstimated(target, PlanningPokerData.MemberName, true);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAnd0Participants_0Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>();
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = null
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            Assert.AreEqual(0, target.Estimations.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndParticipantsListIsNull_EstimationsIsNull()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = null
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);

            Assert.IsNull(target.Estimations);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberInParticipantsAndNotEstimated_CanSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = null
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberInParticipantsAndEstimated_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = 1 }
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoObserverHasEstimated(target);

            Assert.IsNotNull(target.ScrumMaster);
            Assert.IsFalse(target.ScrumMaster.HasEstimated);
            AssertMemberHasEstimated(target, PlanningPokerData.MemberName, true);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberNotInParticipants_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = false }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid(),
                LastMessageId = 22,
                SelectedEstimation = null
            };
            using var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(reconnectTeamResult.SessionId, target.SessionId);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsTrue(target.IsConnected);
            Assert.IsFalse(target.CanCallbackApplication);
            AssertNoMemberHasEstimated(target);
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerStartedTestData))]
        public async Task InitializeTeam_ScrumMasterAndTimerIsStarted_TimerCountdownTimeIsCalculated(DateTime timerEndTime, TimeSpan serviceOffset, TimeSpan expectedRemainingTimerTime)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(expectedRemainingTimerTime, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()));
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerFinishedTestData))]
        public async Task InitializeTeam_ScrumMasterAndTimerIsFinished_TimerCountdownTimeIsZero(DateTime timerEndTime, TimeSpan serviceOffset)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.Zero, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Never);
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerStartedTestData))]
        public async Task InitializeTeam_MemberAndTimerIsStarted_TimerCountdownTimeIsCalculated(DateTime timerEndTime, TimeSpan serviceOffset, TimeSpan expectedRemainingTimerTime)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(expectedRemainingTimerTime, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()));
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerFinishedTestData))]
        public async Task InitializeTeam_MemberAndTimerIsFinished_TimerCountdownTimeIsZero(DateTime timerEndTime, TimeSpan serviceOffset)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.Zero, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Never);
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerStartedTestData))]
        public async Task InitializeTeam_ObserverAndTimerIsStarted_TimerCountdownTimeIsCalculated(DateTime timerEndTime, TimeSpan serviceOffset, TimeSpan expectedRemainingTimerTime)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ObserverName, null);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(expectedRemainingTimerTime, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()));
        }

        [DataTestMethod]
        [DynamicData(nameof(TimerFinishedTestData))]
        public async Task InitializeTeam_ObserverAndTimerIsFinished_TimerCountdownTimeIsZero(DateTime timerEndTime, TimeSpan serviceOffset)
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.TimerEndTime = timerEndTime;
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(serviceOffset);
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());
            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ObserverName, null);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.Zero, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Never);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_NotifyPropertyChanged()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.AreEqual(1, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_SetCredentialsAsync()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials? memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task InitializeTeam_ReconnectTeamResult_SetCredentialsAsync()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials? memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            using var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(reconnectTeamResult, "test member", null);

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task InitializeTeam_TimerIsStarted_RunsTimerFor5Seconds()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc);
            var teamResult = CreateTeamResult(scrumTeam);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 59, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.FromSeconds(-1));
            var timerFactory = new Mock<ITimerFactory>();
            var timerDisposable = new Mock<IDisposable>();
            Action? timerAction = null;
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>()))
                .Callback<Action>(a => timerAction = a)
                .Returns(timerDisposable.Object);

            using var target = CreateController(
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object,
                propertyChangedCounter: propertyChangedCounter);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(5), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.IsNotNull(timerAction);
            Assert.AreEqual(1, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 1, DateTimeKind.Utc));
            timerAction();
            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(3), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(2, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 2, DateTimeKind.Utc));
            timerAction();
            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(2), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(3, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 5, DateTimeKind.Utc));
            timerAction();
            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.Zero, target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(4, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Once());
        }

        [TestMethod]
        public async Task InitializeTeam_TimerDurationIsStored_TimerDurationIsLoaded()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);

            var timerDuration = TimeSpan.FromSeconds(225);
            var timerSettingsRepository = new Mock<ITimerSettingsRepository>();
            timerSettingsRepository.Setup(o => o.GetTimerDurationAsync())
                .ReturnsAsync(timerDuration);
            using var target = CreateController(timerSettingsRepository: timerSettingsRepository.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(timerDuration, target.TimerDuration);
        }

        [TestMethod]
        public async Task InitializeTeam_TimerDurationIsNotStored_TimerDurationIs5Minutes()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);

            var timerSettingsRepository = new Mock<ITimerSettingsRepository>();
            timerSettingsRepository.Setup(o => o.GetTimerDurationAsync())
                .ReturnsAsync(default(TimeSpan?));
            using var target = CreateController(timerSettingsRepository: timerSettingsRepository.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);

            Assert.AreEqual(TimeSpan.FromMinutes(5), target.TimerDuration);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_DisconnectTeam()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.Disconnect();

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_IsConnectedIsFalse()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            Assert.IsFalse(target.IsConnected);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);

            Assert.IsTrue(target.IsConnected);

            await target.Disconnect();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_SetCredentialsToNull()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.Disconnect();

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(null));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_NotifyPropertyChanged()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.Disconnect();

            Assert.AreEqual(2, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.Disconnect();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_DisconnectTeam()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.DisconnectMember(PlanningPokerData.MemberName);

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()));
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task DisconnectMember_ScrumMasterName_ArgumentException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.DisconnectMember(PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task DisconnectMember_Null_ArgumentNullException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(null!));
        }

        [TestMethod]
        public async Task DisconnectMember_Empty_ArgumentNullException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(string.Empty));
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.DisconnectMember(PlanningPokerData.MemberName);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_StartEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartEstimation_CannotStartEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.StartEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_CancelEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CancelEstimation_CannotCancelEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.CancelEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task SelectEstimation_5AndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, 5, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_PositiveInfinityAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(double.PositiveInfinity);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, double.PositiveInfinity, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_NullAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(null);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, null, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_CannotSelectEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task SelectEstimation_Selects5_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(5);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_SelectsPositiveInfinity_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(double.PositiveInfinity);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_SelectsNull_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController();

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(null);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_CanSelectEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<double?>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            var result = target.SelectEstimation(5);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [DataTestMethod]
        [DataRow(Deck.Standard)]
        [DataRow(Deck.Rating)]
        [DataRow(Deck.Tshirt)]
        public async Task ChangeDeck_CanChangeDeck_ChangeDeckOnService(Deck deck)
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            await target.ChangeDeck(deck);

            planningPokerClient.Verify(o => o.ChangeDeck(PlanningPokerData.TeamName, deck, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task ChangeDeck_CannotChangeDeck_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.ChangeDeck(Deck.Standard);

            planningPokerClient.Verify(o => o.ChangeDeck(It.IsAny<string>(), It.IsAny<Deck>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task ChangeDeck_CanChangeDeck_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.ChangeDeck(PlanningPokerData.TeamName, Deck.Fibonacci, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.ChangeDeck(Deck.Fibonacci);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task StartTimer_CanStartTimer_StartTimerOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.StartTimer();

            planningPokerClient.Verify(o => o.StartTimer(PlanningPokerData.TeamName, PlanningPokerData.MemberName, TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartTimer_93SecondsDuration_StartTimerOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            target.TimerDuration = TimeSpan.FromSeconds(93);
            await target.StartTimer();

            planningPokerClient.Verify(o => o.StartTimer(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, TimeSpan.FromSeconds(93), It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartTimer_CannotStartTimer_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ObserverName, null);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartTimer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task StartTimer_CanStartTimer_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.StartTimer(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            var result = target.StartTimer();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task CancelTimer_CanStopTimer_CancelTimerOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc);
            var teamResult = CreateTeamResult(scrumTeam);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 0, DateTimeKind.Utc));
            using var target = CreateController(planningPokerClient: planningPokerClient.Object, dateTimeProvider: dateTimeProvider);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            await target.CancelTimer();

            planningPokerClient.Verify(o => o.CancelTimer(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CancelTimer_CannotStopTimer_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc);
            var teamResult = CreateTeamResult(scrumTeam);
            using var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.ObserverName, null);
            await target.CancelTimer();

            planningPokerClient.Verify(o => o.CancelTimer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CancelTimer_CanStopTimer_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.CancelTimer(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc);
            var teamResult = CreateTeamResult(scrumTeam);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 0, DateTimeKind.Utc));
            using var target = CreateController(
                planningPokerClient: planningPokerClient.Object,
                busyIndicator: busyIndicatorService.Object,
                dateTimeProvider: dateTimeProvider);

            await target.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, null);
            var result = target.CancelTimer();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task TimerDuration_Set_TimerDurationIsStored()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var teamResult = CreateTeamResult(scrumTeam);

            var timerDuration = TimeSpan.FromSeconds(225);
            var timerSettingsRepository = new Mock<ITimerSettingsRepository>();
            timerSettingsRepository.Setup(o => o.SetTimerDurationAsync(It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            using var target = CreateController(timerSettingsRepository: timerSettingsRepository.Object);

            await target.InitializeTeam(teamResult, PlanningPokerData.MemberName, null);
            target.TimerDuration = timerDuration;

            timerSettingsRepository.Verify(o => o.SetTimerDurationAsync(timerDuration));
            Assert.AreEqual(timerDuration, target.TimerDuration);
        }

        internal static void AssertNoMemberHasEstimated(PlanningPokerController controller, bool skipScrumMaster = false)
        {
            if (!skipScrumMaster)
            {
                Assert.IsNotNull(controller.ScrumMaster);
                Assert.IsFalse(controller.ScrumMaster.HasEstimated);
            }

            foreach (var member in controller.Members)
            {
                Assert.IsFalse(member.HasEstimated);
            }

            AssertNoObserverHasEstimated(controller);
        }

        internal static void AssertNoObserverHasEstimated(PlanningPokerController controller)
        {
            foreach (var observer in controller.Observers)
            {
                Assert.IsFalse(observer.HasEstimated);
            }
        }

        internal static void AssertMemberHasEstimated(PlanningPokerController controller, string memberName, bool hasEstimated)
        {
            var memberItem = controller.Members.First(m => m.Name == memberName);
            Assert.AreEqual(hasEstimated, memberItem.HasEstimated);
        }

        private static PlanningPokerController CreateController(
            IPlanningPokerClient? planningPokerClient = null,
            IBusyIndicatorService? busyIndicator = null,
            IMemberCredentialsStore? memberCredentialsStore = null,
            ITimerFactory? timerFactory = null,
            DateTimeProvider? dateTimeProvider = null,
            IServiceTimeProvider? serviceTimeProvider = null,
            ITimerSettingsRepository? timerSettingsRepository = null,
            PropertyChangedCounter? propertyChangedCounter = null)
        {
            if (planningPokerClient == null)
            {
                var planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            if (busyIndicator == null)
            {
                var busyIndicatorMock = new Mock<IBusyIndicatorService>();
                busyIndicator = busyIndicatorMock.Object;
            }

            if (memberCredentialsStore == null)
            {
                var memberCredentialsStoreMock = new Mock<IMemberCredentialsStore>();
                memberCredentialsStore = memberCredentialsStoreMock.Object;
            }

            if (timerFactory == null)
            {
                var timerFactoryMock = new Mock<ITimerFactory>();
                timerFactory = timerFactoryMock.Object;
            }

            if (dateTimeProvider == null)
            {
                dateTimeProvider = new DateTimeProviderMock();
            }

            if (serviceTimeProvider == null)
            {
                var serviceTimeProviderMock = new Mock<IServiceTimeProvider>();
                serviceTimeProvider = serviceTimeProviderMock.Object;
            }

            if (timerSettingsRepository == null)
            {
                var timerSettingsRepositoryMock = new Mock<ITimerSettingsRepository>();
                timerSettingsRepository = timerSettingsRepositoryMock.Object;
            }

            var result = new PlanningPokerController(
                planningPokerClient,
                busyIndicator,
                memberCredentialsStore,
                timerFactory,
                dateTimeProvider,
                serviceTimeProvider,
                timerSettingsRepository);
            if (propertyChangedCounter != null)
            {
                propertyChangedCounter.Target = result;
            }

            return result;
        }

        private static TeamResult CreateTeamResult(ScrumTeam scrumTeam)
        {
            return new TeamResult
            {
                ScrumTeam = scrumTeam,
                SessionId = Guid.NewGuid()
            };
        }
    }
}
