// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumTeamTest
    {
        #region Constructor

        [TestMethod]
        public void Constructor_TeamNameSpecified_TeamNameIsSet()
        {
            // Arrange
            var name = "test team";

            // Act
            var result = new ScrumTeam(name);

            // Verify
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var name = string.Empty;

            // Act
            var result = new ScrumTeam(name);
        }

        #endregion

        #region Observers

        [TestMethod]
        public void Observers_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Observers;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        #endregion

        #region Members

        [TestMethod]
        public void Members_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Members;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        #endregion

        #region ScrumMaster

        [TestMethod]
        public void ScrumMaster_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.ScrumMaster;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScrumMaster_SetScrumMaster_ReturnsNewScrumMasterOfTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster(name);

            // Act
            var result = target.ScrumMaster;

            // Verify
            Assert.AreEqual<ScrumMaster>(master, result);
        }

        #endregion

        #region AvailableEstimations

        [TestMethod]
        public void AvailableEstimations_Get_ReturnsPlanningPokerCardValues()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.AvailableEstimations;

            // Verify
            var expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
        }

        #endregion

        #region State

        [TestMethod]
        public void State_GetAfterConstruction_ReturnsInitial()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.State;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.Initial, result);
        }

        #endregion

        #region EstimationResult

        [TestMethod]
        public void EstimationResult_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            var target = new ScrumTeam("test team");

            // Act
            var result = target.EstimationResult;

            // Verify
            Assert.IsNull(result);
        }

        #endregion

        #region SetScrumMaster

        [TestMethod]
        public void SetScrumMaster_SetName_ReturnsNewScrumMasterOfTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");

            // Act
            var result = target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void SetScrumMaster_NoMembers_ScrumTeamGetsMemberJoinedMessage()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            var result = target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_ObserverAlreadyJoined_ObserverGetsMemberJoinedMessage()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var observer = target.Join("observer", true);

            // Act
            var result = target.SetScrumMaster(name);

            // Verify
            Assert.AreEqual<int>(1, observer.Messages.Count());
            var message = observer.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_ObserverAlreadyJoined_ObserverMessageReceived()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void SetScrumMaster_MemberAlreadyJoined_MemberGetsMemberJoinedMessage()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var member = target.Join("member", false);

            // Act
            var result = target.SetScrumMaster(name);

            // Verify
            Assert.AreEqual<int>(1, member.Messages.Count());
            var message = member.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_MemberAlreadyJoined_MemberMessageReceived()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var member = target.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetScrumMaster_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var name = string.Empty;
            var target = new ScrumTeam("test team");

            // Act
            var result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetScrumMaster_ScrumMasterIsAlreadySet_InvalidOperationException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.SetScrumMaster("master");

            // Act
            var result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetScrumMaster_MemberWithSpecifiedNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join("test", false);

            // Act
            var result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetScrumMaster_ObserverWithSpecifiedNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join("test", true);

            // Act
            var result = target.SetScrumMaster(name);
        }

        #endregion

        #region Join

        [TestMethod]
        public void Join_SetNameAndNotIsObserver_ReturnsNewMemberOfTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Join(name, false);

            // Verify
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Member));
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserver_ReturnsNewObserverOfTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Join(name, true);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<Type>(typeof(Observer), result.GetType());
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Join_NameIsEmpty_ArgumentNullEsception()
        {
            // Arrange
            var name = string.Empty;
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Join(name, false);
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserver_MemberIsInMembersCollection()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Join(name, false);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result }, target.Members.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_MembersAreInMembersCollection()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");

            // Act
            var result1 = target.Join(name1, false);
            var result2 = target.Join(name2, false);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Members.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_ScrumTeamGets2Messages()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

            // Act
            target.Join(name1, false);
            target.Join(name2, false);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            var message1 = eventArgsList[0].Message;
            var message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_ScrumMasterGets2Messages()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");

            // Act
            target.Join(name1, false);
            target.Join(name2, false);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            var message1 = master.Messages.First();
            var message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserver_ObserverIsInObserversCollection()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");

            // Act
            var result = target.Join(name, true);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result }, target.Observers.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ObserversAreInObserversCollection()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");

            // Act
            var result1 = target.Join(name1, true);
            var result2 = target.Join(name2, true);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Observers.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ScrumTeamGets2Messages()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

            // Act
            target.Join(name1, true);
            target.Join(name2, true);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            var message1 = eventArgsList[0].Message;
            var message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ScrumMasterGets2Messages()
        {
            // Arrange
            var name1 = "test1";
            var name2 = "test2";
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");

            // Act
            target.Join(name1, true);
            target.Join(name2, true);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            var message1 = master.Messages.First();
            var message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsMemberAndMemberWithNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            var result = target.Join(name, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsMemberAndObserverWithNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            var result = target.Join(name, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsObserverAndMemberWithNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            var result = target.Join(name, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsObserverAndObserverWithNameExists_ArgumentException()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            var result = target.Join(name, true);
        }

        [TestMethod]
        public void Join_EstimationStarted_OnlyScrumMasterIsInEstimationResult()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            master.StartEstimation();
            var masterEstimation = new Estimation();

            // Act
            var member = (Member)target.Join("member", false);
            master.Estimation = masterEstimation;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
            Assert.IsNotNull(target.EstimationResult);
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(master, masterEstimation)
            };
            CollectionAssert.AreEquivalent(expectedResult, target.EstimationResult.ToList());
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterGetMemberJoinedMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Join_AsMember_ScrumTeamGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");

            // Act
            var result = target.Join("member", false);

            // Verify
            var message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterMessageReceived()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Join_AsMember_MemberDoesNotGetMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsFalse(result.HasMessage);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverGetMemberJoinedMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);

            // Act
            var result = target.Join("member", false);

            // Verify
            var message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverMessageReceived()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_MemberDoesNotGetMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);

            // Act
            var result = target.Join("member", false);

            // Verify
            Assert.IsFalse(result.HasMessage);
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void Disconnect_NameOfTheMember_MemberIsRemovedFromTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            target.Disconnect(name);

            // Verify
            Assert.IsFalse(target.Members.Any());
        }

        [TestMethod]
        public void Disconnect_NameOfTheObserver_ObserverIsRemovedFromTheTeam()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            target.Disconnect(name);

            // Verify
            Assert.IsFalse(target.Observers.Any());
        }

        [TestMethod]
        public void Disconnect_ObserverNorMemberWithTheNameDoNotExist_ObserversAndMembersAreUnchanged()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            var observer = target.Join("observer", true);
            var member = target.Join("member", false);

            // Act
            target.Disconnect(name);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { observer }, target.Observers.ToList());
            CollectionAssert.AreEquivalent(new Observer[] { member }, target.Members.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Disconnect_EmptyName_ArgumentNullException()
        {
            // Arrange
            var name = string.Empty;
            var target = new ScrumTeam("test team");

            // Act
            target.Disconnect(name);
        }

        [TestMethod]
        public void Disconnect_EstimationStarted_OnlyScrumMasterIsInEstimationResult()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = (Member)target.Join("member", false);
            master.StartEstimation();
            var masterEstimation = new Estimation();

            // Act
            target.Disconnect(member.Name);
            master.Estimation = masterEstimation;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
            Assert.IsNotNull(target.EstimationResult);
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(master, masterEstimation),
                new KeyValuePair<Member, Estimation>(member, null)
            };
            CollectionAssert.AreEquivalent(expectedResult, target.EstimationResult.ToList());
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGetMemberDisconnectedMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
            var message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGet2Messages()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
            var member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            var message1 = eventArgsList[0].Message;
            var message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGetMemberDisconnectedMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            TestHelper.ClearMessages(master);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            TestHelper.ClearMessages(master);

            // Act
            target.Disconnect(member.Name);

            // Verify
            var message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGet2Messages()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            var message1 = master.Messages.First();
            var message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterMessageReceived()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Disconnect_AsMember_MemberGetsEmptyMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGetMemberDisconnectedMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            var observer = target.Join("observer", true);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            var observer = target.Join("observer", true);

            // Act
            target.Disconnect(member.Name);

            // Verify
            var message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            var memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverMessageReceived()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            var observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_MemberGetsEmptyMessage()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);
            var observer = target.Join("observer", true);
            TestHelper.ClearMessages(member);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGet2Messages()
        {
            // Arrange
            var target = new ScrumTeam("test team");
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);
            var member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, observer.Messages.Count());
            var message1 = observer.Messages.First();
            var message2 = observer.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        #endregion

        #region FindMemberOrObserver

        [TestMethod]
        public void FindMemberOrObserver_ObserverExists_ReturnsObserver()
        {
            // Arrange
            var name = "observer2";
            var target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            var result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void FindMemberOrObserver_MemberExists_ReturnsMember()
        {
            // Arrange
            var name = "member2";
            var target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            var result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void FindMemberOrObserver_MemberNorObserverExists_ReturnsNull()
        {
            // Arrange
            var name = "test";
            var target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            var result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNull(result);
        }

        #endregion

        #region DisconnectInactiveObservers

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveMembers_TeamIsUnchanged()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var name = "test";
            var target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(1, target.Members.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveMember_MemberIsDisconnected()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var name = "test";
            var target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(0, target.Members.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveObservers_TeamIsUnchanged()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var name = "test";
            var target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(1, target.Observers.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveObserver_ObserverIsDisconnected()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var name = "test";
            var target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(0, target.Observers.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_ActiveMemberAndInactiveObserver_MemberMessageReceived()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new ScrumTeam("test team", dateTimeProvider);
            var master = target.SetScrumMaster("master");
            var observer = target.Join("observer", true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            var member = target.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(member.HasMessage);
            var message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void DisconnectInactiveObservers_ActiveObserverAndInactiveMember_ObserverMessageReceived()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new ScrumTeam("test team", dateTimeProvider);
            var master = target.SetScrumMaster("master");
            var member = target.Join("member", false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            var observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(observer.HasMessage);
            var message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void DisconnectInactiveObservers_EstimationStartedActiveScrumMasterInactiveMember_ScrumMasterGetsEstimationResult()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new ScrumTeam("test team", dateTimeProvider);
            var master = target.SetScrumMaster("master");
            var member = (Member)target.Join("member", false);
            master.StartEstimation();

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            master.Estimation = new Estimation();
            master.UpdateActivity();

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            TestHelper.ClearMessages(master);

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimationFinished, target.State);
            Assert.IsNotNull(target.EstimationResult);

            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            var message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.EstimationEnded, message.MessageType);
        }

        #endregion
    }
}
