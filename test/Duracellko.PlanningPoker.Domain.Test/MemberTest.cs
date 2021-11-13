using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class MemberTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var name = "test";

            // Act
            var result = new Member(team, name);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void Constructor_SessionId_ZeroGuid()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var name = "test";

            // Act
            var result = new Member(team, name);

            // Verify
            Assert.AreEqual<Guid>(Guid.Empty, result.SessionId);
        }

        [TestMethod]
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            var name = "test";

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new Member(null!, name));
        }

        [TestMethod]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var team = new ScrumTeam("test team");

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new Member(team, string.Empty));
        }

        [TestMethod]
        public void Estimation_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Member(team, "test");

            // Act
            var result = target.Estimation;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Estimation_SetAndGet_ReturnsTheValue()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var estimation = new Estimation(2);
            var target = new Member(team, "test");

            // Act
            target.Estimation = estimation;
            var result = target.Estimation;

            // Verify
            Assert.AreEqual<Estimation>(estimation, result);
        }

        [TestMethod]
        public void Estimation_SetTwiceAndGet_ReturnsTheValue()
        {
            // Arrange
            var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var estimation = new Estimation(21);
            var target = new Member(team, "test");

            // Act
            target.Estimation = estimation;
            target.Estimation = estimation;
            var result = target.Estimation;

            // Verify
            Assert.AreEqual<Estimation>(estimation, result);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_StateChangedToEstimationFinished()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(double.PositiveInfinity);
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationFinished, team.State);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGenerated()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(8);
            var memberEstimation = new Estimation(20);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(team.EstimationResult);
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(master, masterEstimation),
                new KeyValuePair<Member, Estimation>(member, memberEstimation),
            };
            CollectionAssert.AreEquivalent(expectedResult, team.EstimationResult.ToList());
        }

        [TestMethod]
        public void Estimation_SetOnAllMembersWithCustomValues_EstimationResultIsGenerated()
        {
            // Arrange
            var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(22.34);
            var memberEstimation = new Estimation(-100.2);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(team.EstimationResult);
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(master, masterEstimation),
                new KeyValuePair<Member, Estimation>(member, memberEstimation),
            };
            CollectionAssert.AreEquivalent(expectedResult, team.EstimationResult.ToList());
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_StateIsEstimationInProgress()
        {
            // Arrange
            var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(89);

            // Act
            master.Estimation = masterEstimation;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_EstimationResultIsNull()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var memberEstimation = new Estimation(100);

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNull(team.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetTwiceToDifferentValues_EstimationIsChanged()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var estimation1 = new Estimation(8);
            var estimation2 = new Estimation(13);
            var target = new Member(team, "test");
            target.Estimation = estimation1;

            // Act
            target.Estimation = estimation2;

            // Verify
            Assert.AreEqual(estimation2, target.Estimation);
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_ScrumTeamGetMemberEstimatedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            MessageReceivedEventArgs? eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var memberEstimation = new Estimation(1);

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ScrumTeamGetEstimationEndedMessage()
        {
            // Arrange
            var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            MessageReceivedEventArgs? eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var masterEstimation = new Estimation(55);
            var memberEstimation = new Estimation(55);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForScrumTeam()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            MessageReceivedEventArgs? eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var masterEstimation = new Estimation(20);
            var memberEstimation = new Estimation(3);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ScrumMasterGetEstimationEndedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            master.ClearMessages();
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(master.HasMessage);
            Assert.AreEqual(3, master.Messages.Count());
            var message = master.Messages.Last();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForScrumMaster()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            master.ClearMessages();
            var masterEstimation = new Estimation(double.PositiveInfinity);
            var memberEstimation = new Estimation(double.PositiveInfinity);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(master.HasMessage);
            Assert.AreEqual(3, master.Messages.Count());
            var message = master.Messages.Last();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ScrumMasterMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            master.ClearMessages();
            var masterEstimation = new Estimation(5);
            var memberEstimation = new Estimation(40);
            EventArgs? eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_ScrumMasterGetsMemberEstimatedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            master.ClearMessages();
            var memberEstimation = new Estimation(3);

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_MemberGetEstimationEndedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(1);
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.ClearMessages();
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(member.HasMessage);
            Assert.AreEqual(2, member.Messages.Count());
            var message = member.Messages.Last();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForMember()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation(1);
            var memberEstimation = new Estimation(1);

            // Act
            master.Estimation = masterEstimation;
            member.ClearMessages();
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(member.HasMessage);
            Assert.AreEqual(2, member.Messages.Count());
            var message = member.Messages.Last();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_MemberMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            member.ClearMessages();
            var masterEstimation = new Estimation(5);
            var memberEstimation = new Estimation(8);
            EventArgs? eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_MemberGetsMemberEstimatedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            member.ClearMessages();
            var memberEstimation = new Estimation(1);

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ObserverGetEstimationEndedMessage()
        {
            // Arrange
            var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var observer = team.Join("observer", true);
            master.StartEstimation();
            var masterEstimation = new Estimation(21);
            var memberEstimation = new Estimation(34);

            // Act
            master.Estimation = masterEstimation;
            observer.ClearMessages();
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Assert.AreEqual(2, observer.Messages.Count());
            var message = observer.Messages.Last();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForObserver()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var observer = team.Join("observer", true);
            master.StartEstimation();
            var masterEstimation = new Estimation(20);
            var memberEstimation = new Estimation(40);

            // Act
            master.Estimation = masterEstimation;
            observer.ClearMessages();
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Assert.AreEqual(2, observer.Messages.Count());
            var message = observer.Messages.Last();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult?>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ObserverMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var observer = team.Join("observer", true);
            master.StartEstimation();
            observer.ClearMessages();
            var masterEstimation = new Estimation(5);
            var memberEstimation = new Estimation(5);
            EventArgs? eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_ObserverGetsMemberEstimatedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var observer = team.Join("observer", true);
            master.StartEstimation();
            observer.ClearMessages();
            var memberEstimation = new Estimation();

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimation_SetToNotAvailableValueWithStandardValues_ArgumentException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            var masterEstimation = new Estimation(55.0);

            // Act
            Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
        }

        [TestMethod]
        public void Estimation_SetToNotAvailableValueWithFibonacciValues_ArgumentException()
        {
            // Arrange
            var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            var masterEstimation = new Estimation(40.0);

            // Act
            Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
        }

        [TestMethod]
        public void Estimation_SetToNotAvailableValueWithCustomValues_ArgumentException()
        {
            // Arrange
            var availableEstimations = ScrumTeamTestData.GetCustomEstimationDeck();
            var team = ScrumTeamTestData.CreateScrumTeam("test team", availableEstimations: availableEstimations);
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            var masterEstimation = new Estimation(double.PositiveInfinity);

            // Act
            Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
        }
    }
}
