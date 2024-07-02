using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class TimerMessageTest
{
    [TestMethod]
    public void Constructor_TypeSpecified_MessageTypeIsSet()
    {
        // Arrange
        var type = MessageType.TimerStarted;
        var endTime = new DateTime(2021, 11, 16, 23, 49, 31, DateTimeKind.Utc);

        // Act
        var result = new TimerMessage(type, endTime);

        // Assert
        Assert.AreEqual<MessageType>(type, result.MessageType);
        Assert.AreEqual<DateTime>(endTime, result.EndTime);
    }
}
