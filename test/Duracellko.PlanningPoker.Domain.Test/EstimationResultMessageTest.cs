using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class EstimationResultMessageTest
{
    [TestMethod]
    public void Constructor_TypeSpecified_MessageTypeIsSet()
    {
        // Arrange
        var type = MessageType.EstimationEnded;
        var estimationResult = new EstimationResult([]);

        // Act
        var result = new EstimationResultMessage(type, estimationResult);

        // Verify
        Assert.AreEqual<MessageType>(type, result.MessageType);
        Assert.AreEqual<EstimationResult>(estimationResult, result.EstimationResult);
    }

    [TestMethod]
    public void Constructor_EstimationResultIsNull_ArgumentNullException()
    {
        // Arrange
        var type = MessageType.EstimationEnded;

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => new EstimationResultMessage(type, null!));
    }
}
