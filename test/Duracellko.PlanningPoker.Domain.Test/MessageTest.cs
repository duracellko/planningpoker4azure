using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class MessageTest
{
    [TestMethod]
    public void Constructor_TypeSpecified_MessageTypeIsSet()
    {
        // Arrange
        var type = MessageType.EstimationStarted;

        // Act
        var result = new Message(type);

        // Verify
        Assert.AreEqual<MessageType>(type, result.MessageType);
    }
}
