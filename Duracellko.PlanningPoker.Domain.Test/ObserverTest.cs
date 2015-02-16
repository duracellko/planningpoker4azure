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
    public class ObserverTest
    {
        #region Constructor

        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var name = "test";

            // Act
            var result = new Observer(team, name);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            var name = "test";

            // Act
            var result = new Observer(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var team = new ScrumTeam("test team");

            // Act
            var result = new Observer(team, string.Empty);
        }

        #endregion

        #region HasMessages

        [TestMethod]
        public void HasMessages_GetAfterConstruction_ReturnsFalse()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.HasMessage;

            // Verify
            Assert.IsFalse(result);
        }

        #endregion

        #region Messages

        [TestMethod]
        public void Messages_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.Messages;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        #endregion

        #region PopMessage

        [TestMethod]
        public void PopMessage_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.PopMessage();

            // Verify
            Assert.IsNull(result);
        }

        #endregion

        #region ClearMessages

        [TestMethod]
        public void ClearMessages_AfterConstruction_ReturnsZero()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new Observer(team, "test");

            // Act
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(0, result);
        }

        [TestMethod]
        public void ClearMessages_After2Messages_HasNoMessages()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var target = team.Join("test", true);
            master.StartEstimation();
            master.CancelEstimation();

            // Act
            var result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(2, result);
            Assert.IsFalse(target.HasMessage);
        }

        #endregion

        #region LastActivity

        [TestMethod]
        public void LastActivity_AfterConstruction_ReturnsUtcNow()
        {
            // Arrange
            var utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(utcNow);

            var team = new ScrumTeam("test team", dateTimeProvider);
            var target = new Observer(team, "test");

            // Act
            var result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }

        #endregion

        #region LastActivity

        [TestMethod]
        public void UpdateActivity_UtcNowIsChanged_LastActivityIsChanged()
        {
            // Arrange
            var utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 2, 3, 35, 0));

            var team = new ScrumTeam("test team", dateTimeProvider);
            var target = new Observer(team, "test");
            dateTimeProvider.SetUtcNow(utcNow);

            // Act
            target.UpdateActivity();
            var result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }

        #endregion
    }
}
