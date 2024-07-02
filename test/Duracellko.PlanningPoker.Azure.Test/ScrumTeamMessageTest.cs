using System;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test;

[TestClass]
public class ScrumTeamMessageTest
{
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
    public void Constructor_TeamNameIsEmpty_ArgumentNullException()
    {
        // Arrange
        var teamName = string.Empty;
        var messageType = MessageType.Empty;

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => new ScrumTeamMessage(teamName, messageType));
    }
}
