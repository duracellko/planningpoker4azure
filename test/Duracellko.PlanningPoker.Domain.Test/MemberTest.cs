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
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            var name = "test";

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new Member(null, name));
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
            var estimation = new Estimation();
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
            var team = new ScrumTeam("test team");
            var estimation = new Estimation();
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
            var masterEstimation = new Estimation();
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
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

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
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation();

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
            var memberEstimation = new Estimation();

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNull(team.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetTwiceToDifferentValues_InvalidOperationException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var estimation1 = new Estimation();
            var estimation2 = new Estimation();
            var target = new Member(team, "test");
            target.Estimation = estimation1;

            // Act
            target.Estimation = estimation2;
        }

        [TestMethod]
        public void Estimation_SetOnMemberOnly_ScrumTeamGetMemberEstimatedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var memberEstimation = new Estimation();

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
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

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
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ScrumMasterGetEstimationEndedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            TestHelper.ClearMessages(master);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForScrumMaster()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            TestHelper.ClearMessages(master);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            member.Estimation = memberEstimation;

            // Verify
            master.PopMessage();
            master.PopMessage();
            var message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ScrumMasterMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            TestHelper.ClearMessages(master);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();
            EventArgs eventArgs = null;
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
            TestHelper.ClearMessages(master);
            var memberEstimation = new Estimation();

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
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
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            TestHelper.ClearMessages(member);
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(member.HasMessage);
            member.PopMessage();
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_EstimationResultIsGeneratedForMember()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            TestHelper.ClearMessages(member);
            member.Estimation = memberEstimation;

            // Verify
            member.PopMessage();
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult>(team.EstimationResult, estimationResultMessage.EstimationResult);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_MemberMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            master.StartEstimation();
            TestHelper.ClearMessages(member);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();
            EventArgs eventArgs = null;
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
            TestHelper.ClearMessages(member);
            var memberEstimation = new Estimation();

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimation_SetOnAllMembers_ObserverGetEstimationEndedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var observer = team.Join("observer", true);
            master.StartEstimation();
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            TestHelper.ClearMessages(observer);
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            observer.PopMessage();
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
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
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();

            // Act
            master.Estimation = masterEstimation;
            TestHelper.ClearMessages(observer);
            member.Estimation = memberEstimation;

            // Verify
            observer.PopMessage();
            var message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimationResultMessage));
            var estimationResultMessage = (EstimationResultMessage)message;
            Assert.AreEqual<EstimationResult>(team.EstimationResult, estimationResultMessage.EstimationResult);
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
            TestHelper.ClearMessages(observer);
            var masterEstimation = new Estimation();
            var memberEstimation = new Estimation();
            EventArgs eventArgs = null;
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
            TestHelper.ClearMessages(observer);
            var memberEstimation = new Estimation();

            // Act
            member.Estimation = memberEstimation;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimation_SetToNotAvailableValue_ArgumentException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            var masterEstimation = new Estimation(44.0);

            // Act
            Assert.ThrowsException<ArgumentException>(() => master.Estimation = masterEstimation);
        }
    }
}
