using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

[TestClass]
public class MessageReceivedEventArgsTest
{
    [TestMethod]
    public void Constructor_Message_MessagePropertyIsSet()
    {
        // Arrange
        var message = new Message(MessageType.Empty);

        // Act
        var result = new MessageReceivedEventArgs(message);

        // Verify
        Assert.AreEqual(message, result.Message);
    }
}
