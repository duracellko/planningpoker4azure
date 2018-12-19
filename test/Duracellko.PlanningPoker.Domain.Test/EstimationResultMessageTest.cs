using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationResultMessageTest
    {
        [TestMethod]
        public void Constructor_TypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            var type = MessageType.EstimationEnded;

            // Act
            var result = new EstimationResultMessage(type);

            // Verify
            Assert.AreEqual<MessageType>(type, result.MessageType);
        }

        [TestMethod]
        public void EstimationResult_Set_EstimationResultIsSet()
        {
            // Arrange
            var target = new EstimationResultMessage(MessageType.EstimationEnded);
            var estimationResult = new EstimationResult(Enumerable.Empty<Member>());

            // Act
            target.EstimationResult = estimationResult;

            // Verify
            Assert.AreEqual<EstimationResult>(estimationResult, target.EstimationResult);
        }
    }
}
