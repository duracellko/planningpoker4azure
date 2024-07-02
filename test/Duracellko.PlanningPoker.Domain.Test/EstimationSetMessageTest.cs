using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class EstimationSetMessageTest
{
    [TestMethod]
    public void Constructor_TypeSpecified_MessageTypeIsSet()
    {
        // Arrange
        var type = MessageType.AvailableEstimationsChanged;
        var estimations = DeckProvider.Default.GetDefaultDeck();

        // Act
        var result = new EstimationSetMessage(type, estimations);

        // Verify
        Assert.AreEqual(type, result.MessageType);
        Assert.AreEqual(estimations, result.Estimations);
    }

    [TestMethod]
    public void Constructor_EstimationSetIsNull_ArgumentNullException()
    {
        // Arrange
        var type = MessageType.AvailableEstimationsChanged;

        // Act
        Assert.ThrowsException<ArgumentNullException>(() => new EstimationSetMessage(type, null!));
    }
}
