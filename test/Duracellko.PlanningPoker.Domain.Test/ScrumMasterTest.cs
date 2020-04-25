using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumMasterTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var name = "test";

            // Act
            var result = new ScrumMaster(team, name);

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
            Assert.ThrowsException<ArgumentNullException>(() => new ScrumMaster(null, name));
        }

        [TestMethod]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var team = new ScrumTeam("test team");

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new ScrumMaster(team, string.Empty));
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_StateChangedToEstimationInProgress()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");

            // Act
            master.StartEstimation();

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationInProgress, team.State);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_ScrumTeamGotMessageEstimationStarted()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_ScrumMasterGotMessageEstimationStarted()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_ScrumMasterReceivedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_MemberGotMessageEstimationStarted()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void StartEstimation_MemberHasEstimation_MembersEstimationIsReset()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            member.Estimation = new Estimation();

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNull(member.Estimation);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_MemberReceivedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_ObserverGotMessageEstimationStarted()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_ObserverReceivedMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", false);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartEstimation_EstimationInProgress_InvalidOperationException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => master.StartEstimation());
        }

        [TestMethod]
        public void StartEstimation_EstimationNotStarted_EstimationResultSetToNull()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNull(team.EstimationResult);
        }

        [TestMethod]
        public void StartEstimation_EstimationFinished_EstimationResultSetToNull()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            master.Estimation = new Estimation();

            // Act
            master.StartEstimation();

            // Verify
            Assert.IsNull(team.EstimationResult);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_StateChangedToEstimationCanceled()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();

            // Act
            master.CancelEstimation();

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationCanceled, team.State);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ScrumTeamGetMessageEstimationCanceled()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ScrumTeamGet2Messages()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var eventArgsList = new List<MessageReceivedEventArgs>();
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
            master.StartEstimation();

            // Act
            master.CancelEstimation();

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            var message1 = eventArgsList[0].Message;
            var message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
        }

        [TestMethod]
        public void CancelEstimation_EstimationNotStarted_ScrumTeamGetNoMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ScrumMasterGetMessageEstimationCanceled()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            TestHelper.ClearMessages(master);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ScrumMasterGet2Messages()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();

            // Act
            master.CancelEstimation();

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            var message1 = master.Messages.First();
            var message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ScrumMasterMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            master.StartEstimation();
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimation_EstimationNotStarted_ScrumMasterGetNoMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_MemberGetMessageEstimationCanceled()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);
            master.StartEstimation();
            TestHelper.ClearMessages(member);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_MemberGet2Messages()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);
            master.StartEstimation();

            // Act
            master.CancelEstimation();

            // Verify
            Assert.AreEqual<int>(2, member.Messages.Count());
            var message1 = member.Messages.First();
            var message2 = member.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_MemberMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);
            master.StartEstimation();
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimation_EstimationNotStarted_MemberGetNoMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = team.Join("member", false);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ObserverGetMessageEstimationCanceled()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);
            master.StartEstimation();
            TestHelper.ClearMessages(observer);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ObserverGet2Message()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);
            master.StartEstimation();

            // Act
            master.CancelEstimation();

            // Verify
            Assert.AreEqual<int>(2, observer.Messages.Count());
            var message1 = observer.Messages.First();
            var message2 = observer.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimationStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimationCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimation_EstimationInProgress_ObserverMessageReceived()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);
            master.StartEstimation();
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimation_EstimationNotStarted_ObserverGetNoMessage()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);

            // Act
            master.CancelEstimation();

            // Verify
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void UpdateActivity_IsDormant_IsNotDormant()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var observer = team.Join("observer", true);

            // Act
            team.Disconnect(master.Name);
            Assert.IsTrue(master.IsDormant);
            master.UpdateActivity();

            // Verify
            Assert.IsFalse(master.IsDormant);
        }
    }
}
