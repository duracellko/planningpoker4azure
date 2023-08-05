using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class PlanningPokerControllerTestMessages
    {
        private CultureInfo? _originalCultureInfo;

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
        public async Task ProcessMessages_EmptyCollection_LastMessageIdIsNotUpdated()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            target.ProcessMessages(Enumerable.Empty<Message>());

            Assert.AreEqual(0, propertyChangedCounter.Count);
            Assert.AreEqual(-1, target.LastMessageId);
        }

        [TestMethod]
        public async Task ProcessMessages_EmptyMessage_LastMessageIdIsUpdated()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new Message
            {
                Id = 4,
                Type = MessageType.Empty
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberJoinedWithMember_2Members()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "New member"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            var expectedMembers = new string[] { "New member", PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberJoinedWithObserver_1Observer()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Observers = null!;
            scrumTeam.State = TeamState.EstimationFinished;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ObserverType,
                    Name = "New observer"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            var expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { "New observer" };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_2xMemberJoinedWithMember_3Members()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message1 = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "XYZ"
                }
            };
            var message2 = new MemberMessage
            {
                Id = 2,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "New member"
                }
            };
            target.ProcessMessages(new Message[] { message1, message2 });

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(2, target.LastMessageId);
            var expectedMembers = new string[] { "New member", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithScrumMaster_ScrumMasterIsNull()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ObserverName, null);

            var message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.IsNull(target.ScrumMaster);
            var expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target, skipScrumMaster: true);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithMember_1Member()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var member = new TeamMember
            {
                Type = PlanningPokerData.MemberType,
                Name = "New member",
            };
            scrumTeam.Members.Add(member);
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.IsNotNull(target.ScrumMaster);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster.Name);
            var expectedMembers = new string[] { "New member" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithObserver_0Observers()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ObserverType,
                    Name = PlanningPokerData.ObserverName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumMaster);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster.Name);
            var expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = Array.Empty<string>();
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithNotExistingName_NoChanges()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Disconnect"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.IsNotNull(target.ScrumMaster);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster.Name);
            var expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.Select(m => m.Name).ToList());
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimationStartedAndStateIsInitialize_StateIsEstimationInProgress()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new Message
            {
                Id = 1,
                Type = MessageType.EstimationStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimationStartedAndStateIsEstimationFinished_StateIsEstimationInProgress()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new Message
            {
                Id = 2,
                Type = MessageType.EstimationStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(2, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimationStartedAndStateIsEstimationCanceled_StateIsEstimationInProgress()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new Message
            {
                Id = 3,
                Type = MessageType.EstimationStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(3, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimationCanceled_StateIsEstimationCanceled()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new Message
            {
                Id = 4,
                Type = MessageType.EstimationCanceled
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationCanceled, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberEstimatedWithMember_1Estimation()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message = new MemberMessage
            {
                Id = 3,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            StartEstimation(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(3, target.LastMessageId);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);

            Assert.IsNotNull(target.Estimations);
            Assert.AreEqual(1, target.Estimations.Count());
            var estimation = target.Estimations.First();
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            PlanningPokerControllerTest.AssertNoObserverHasEstimated(target);

            Assert.IsNotNull(target.ScrumMaster);
            Assert.IsFalse(target.ScrumMaster.HasEstimated);
            PlanningPokerControllerTest.AssertMemberHasEstimated(target, PlanningPokerData.MemberName, true);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberEstimatedWithScrumMaster_1Estimation()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ObserverName, null);

            var message = new MemberMessage
            {
                Id = 4,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            StartEstimation(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoObserverHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            Assert.AreEqual(1, target.Estimations.Count());
            var estimation = target.Estimations.First();
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);

            Assert.IsNotNull(target.ScrumMaster);
            Assert.IsTrue(target.ScrumMaster.HasEstimated);
            PlanningPokerControllerTest.AssertMemberHasEstimated(target, PlanningPokerData.MemberName, false);
        }

        [TestMethod]
        public async Task ProcessMessages_2xMemberEstimated_2Estimations()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message1 = new MemberMessage
            {
                Id = 5,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            var message2 = new MemberMessage
            {
                Id = 6,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            StartEstimation(target);
            target.ProcessMessages(new Message[] { message2, message1 });

            Assert.AreEqual(3, propertyChangedCounter.Count);
            Assert.AreEqual(6, target.LastMessageId);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoObserverHasEstimated(target);

            Assert.IsNotNull(target.Estimations);
            var estimations = target.Estimations.ToList();
            Assert.AreEqual(2, estimations.Count);
            var estimation = estimations[0];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            estimation = estimations[1];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);

            Assert.IsNotNull(target.ScrumMaster);
            Assert.IsTrue(target.ScrumMaster.HasEstimated);
            PlanningPokerControllerTest.AssertMemberHasEstimated(target, PlanningPokerData.MemberName, true);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimationEnded_5Estimations()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            var message1 = new MemberMessage
            {
                Id = 5,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Developer 1"
                }
            };
            var message2 = new MemberMessage
            {
                Id = 6,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            var message3 = new MemberMessage
            {
                Id = 7,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            var message4 = new MemberMessage
            {
                Id = 8,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Tester"
                }
            };
            var message5 = new MemberMessage
            {
                Id = 9,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Developer 2"
                }
            };

            var message = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
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
                }
            };

            StartEstimation(target);
            ProcessMessage(target, message1);
            target.ProcessMessages(new Message[] { message2, message3, message4 });
            target.ProcessMessages(new Message[] { message5, message });

            Assert.AreEqual(7, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsTrue(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

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
        public async Task ProcessMessages_EstimationEndedAndMemberWithoutEstimation_4Estimations()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
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
                }
            };

            StartEstimation(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

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
        public async Task ProcessMessages_EstimationEndedAndSameEstimationCount_6Estimations()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
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
                }
            };

            StartEstimation(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

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
        public async Task ProcessMessages_EstimationEndedAndSameEstimationCountWithNull_6Estimations()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new EstimationResultMessage
            {
                Id = 10,
                Type = MessageType.EstimationEnded,
                EstimationResult = new List<EstimationResultItem>
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
                }
            };

            StartEstimation(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

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
        public async Task ProcessMessages_AvailableEstimationsChanged_AvailableEstimationsAreChanged()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new EstimationSetMessage
            {
                Id = 5,
                Type = MessageType.AvailableEstimationsChanged,
                Estimations = new List<Estimation>
                {
                    new Estimation { Value = 0 },
                    new Estimation { Value = 0.5 },
                    new Estimation { Value = 1 },
                    new Estimation { Value = 2 },
                    new Estimation { Value = 3 },
                    new Estimation { Value = 5 },
                    new Estimation { Value = 100 },
                    new Estimation { Value = double.PositiveInfinity },
                    new Estimation()
                }
            };
            target.ProcessMessages(new Message[] { message });

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(5, target.LastMessageId);

            var availableEstimations = target.ScrumTeam!.AvailableEstimations;
            Assert.AreEqual(9, availableEstimations.Count);
            Assert.AreEqual(0.0, availableEstimations[0].Value);
            Assert.AreEqual(0.5, availableEstimations[1].Value);
            Assert.AreEqual(1.0, availableEstimations[2].Value);
            Assert.AreEqual(2.0, availableEstimations[3].Value);
            Assert.AreEqual(3.0, availableEstimations[4].Value);
            Assert.AreEqual(5.0, availableEstimations[5].Value);
            Assert.AreEqual(100.0, availableEstimations[6].Value);
            Assert.AreEqual(double.PositiveInfinity, availableEstimations[7].Value);
            Assert.IsNull(availableEstimations[8].Value);

            Assert.AreEqual(TeamState.Initial, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
        }

        [TestMethod]
        public async Task ProcessMessages_TimerStartedAndCountdownTimerIsNotActive_RunsTimerFor2Seconds()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 59, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.FromSeconds(2));
            var timerFactory = new Mock<ITimerFactory>();
            var timerDisposable = new Mock<IDisposable>();
            Action? timerAction = null;
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>()))
                .Callback<Action>(a => timerAction = a)
                .Returns(timerDisposable.Object);

            using var target = CreateController(
                propertyChangedCounter,
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            var message = new TimerMessage
            {
                Id = 1,
                Type = MessageType.TimerStarted,
                EndTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc)
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationInProgress, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(2), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.IsNotNull(timerAction);
            Assert.AreEqual(1, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 0, DateTimeKind.Utc));
            timerAction();
            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(1), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(2, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 1, DateTimeKind.Utc));
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.Zero);
            timerAction();
            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(2), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(3, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Never());

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
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
        public async Task ProcessMessages_TimerStartedAndCountdownTimerIsActive_RunsTimerFor2Seconds()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 32, DateTimeKind.Utc);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 59, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.FromSeconds(-5));
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>())).Returns(Mock.Of<IDisposable>());

            using var target = CreateController(
                propertyChangedCounter,
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var message = new TimerMessage
            {
                Id = 1,
                Type = MessageType.TimerStarted,
                EndTime = new DateTime(2021, 11, 18, 10, 24, 18, DateTimeKind.Utc)
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.Initial, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

            Assert.IsFalse(target.CanStartTimer);
            Assert.IsTrue(target.CanStopTimer);
            Assert.IsFalse(target.CanChangeTimer);
            Assert.AreEqual(TimeSpan.FromSeconds(200), target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(1, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task ProcessMessages_TimerCanceledAndCountdownTimerIsActive_StopsCountdownTimer()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.TimerEndTime = new DateTime(2021, 11, 18, 10, 21, 32, DateTimeKind.Utc);

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 59, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.FromSeconds(-5));
            var timerFactory = new Mock<ITimerFactory>();
            var timerDisposable = new Mock<IDisposable>();
            Action? timerAction = null;
            timerFactory.Setup(o => o.StartTimer(It.IsAny<Action>()))
                .Callback<Action>(a => timerAction = a)
                .Returns(timerDisposable.Object);

            using var target = CreateController(
                propertyChangedCounter,
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var message = new Message
            {
                Id = 1,
                Type = MessageType.TimerCanceled
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.EstimationFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsNull(target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Once());
            Assert.AreEqual(1, propertyChangedCounter.Count);
            timerDisposable.Verify(o => o.Dispose(), Times.Once());
        }

        [TestMethod]
        public async Task ProcessMessages_TimerCanceledAndCountdownTimerIsNotActive_NoAction()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.Initial;

            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 20, 59, DateTimeKind.Utc));
            var serviceTimeProvider = new Mock<IServiceTimeProvider>();
            serviceTimeProvider.SetupGet(o => o.ServiceTimeOffset).Returns(TimeSpan.FromSeconds(-5));
            var timerFactory = new Mock<ITimerFactory>();

            using var target = CreateController(
                propertyChangedCounter,
                timerFactory: timerFactory.Object,
                dateTimeProvider: dateTimeProvider,
                serviceTimeProvider: serviceTimeProvider.Object);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.MemberName, null);

            dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc));
            var message = new Message
            {
                Id = 1,
                Type = MessageType.TimerCanceled
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, target.LastMessageId);
            Assert.IsNotNull(target.ScrumTeam);
            Assert.AreEqual(TeamState.Initial, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanShowEstimationSummary);
            Assert.IsNull(target.EstimationSummary);
            Assert.IsFalse(target.CanChangeDeck);
            Assert.IsFalse(target.CanCallbackApplication);
            PlanningPokerControllerTest.AssertNoMemberHasEstimated(target);

            Assert.IsTrue(target.CanStartTimer);
            Assert.IsFalse(target.CanStopTimer);
            Assert.IsTrue(target.CanChangeTimer);
            Assert.IsNull(target.RemainingTimerTime);
            timerFactory.Verify(o => o.StartTimer(It.IsAny<Action>()), Times.Never());
            Assert.AreEqual(1, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task ProcessMessages_Null_ArgumentNullException()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            using var target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(CreateTeamResult(scrumTeam), PlanningPokerData.ScrumMasterName, null);

            Assert.ThrowsException<ArgumentNullException>(() => target.ProcessMessages(null!));
        }

        private static PlanningPokerController CreateController(
            PropertyChangedCounter? propertyChangedCounter = null,
            ITimerFactory? timerFactory = null,
            DateTimeProvider? dateTimeProvider = null,
            IServiceTimeProvider? serviceTimeProvider = null)
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var busyIndicator = new Mock<IBusyIndicatorService>();
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var timerSettingsRepository = new Mock<ITimerSettingsRepository>();

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

            var result = new PlanningPokerController(
                planningPokerClient.Object,
                busyIndicator.Object,
                memberCredentialsStore.Object,
                timerFactory,
                dateTimeProvider,
                serviceTimeProvider,
                timerSettingsRepository.Object);
            if (propertyChangedCounter != null)
            {
                // Subtract 1 PropertyChanged event raised by InitializeTeam
                propertyChangedCounter.Count = -1;
                propertyChangedCounter.Target = result;
            }

            return result;
        }

        private static void ProcessMessage(PlanningPokerController controller, Message message)
        {
            controller.ProcessMessages(new Message[] { message });
        }

        private static void StartEstimation(PlanningPokerController controller)
        {
            var message = new Message
            {
                Id = 1,
                Type = MessageType.EstimationStarted
            };
            ProcessMessage(controller, message);
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
