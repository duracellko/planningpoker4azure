using System;
using System.Linq;
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
        var estimationResult = new EstimationResult(Enumerable.Empty<Member>());

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
        Assert.ThrowsException<ArgumentNullException>(() => new EstimationResultMessage(type, null!));
    }
}
