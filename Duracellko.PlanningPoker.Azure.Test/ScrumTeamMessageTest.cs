// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class ScrumTeamMessageTest
    {
        #region Constructor

        [TestMethod]
        public void Constructor_TeamNameSpecified_TeamNameIsSet()
        {
            // Arrange
            var teamName = "test";
            var messageType = MessageType.Empty;

            // Act
            var result = new ScrumTeamMessage(teamName, messageType);

            // Verify
            Assert.AreEqual<string>(teamName, result.TeamName);
        }

        [TestMethod]
        public void Constructor_MessageTypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            var teamName = "test";
            var messageType = MessageType.MemberJoined;

            // Act
            var result = new ScrumTeamMessage(teamName, messageType);

            // Verify
            Assert.AreEqual<MessageType>(messageType, result.MessageType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var teamName = string.Empty;
            var messageType = MessageType.Empty;

            // Act
            var result = new ScrumTeamMessage(teamName, messageType);
        }

        #endregion
    }
}
