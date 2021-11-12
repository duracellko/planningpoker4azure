using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class MemberMessageTest
    {
        [TestMethod]
        public void Constructor_TypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            var type = MessageType.MemberJoined;
            var team = new ScrumTeam("test team");
            var member = new Member(team, "test");

            // Act
            var result = new MemberMessage(type, member);

            // Verify
            Assert.AreEqual<MessageType>(type, result.MessageType);
            Assert.AreEqual<Observer>(member, result.Member);
        }
    }
}
