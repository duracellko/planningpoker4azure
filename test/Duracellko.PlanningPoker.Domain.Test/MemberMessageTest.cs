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

            // Act
            var result = new MemberMessage(type);

            // Verify
            Assert.AreEqual<MessageType>(type, result.MessageType);
        }

        [TestMethod]
        public void Member_Set_MemberIsSet()
        {
            // Arrange
            var target = new MemberMessage(MessageType.MemberJoined);
            var team = new ScrumTeam("test team");
            var member = new Member(team, "test");

            // Act
            target.Member = member;

            // Verify
            Assert.AreEqual<Observer>(member, target.Member);
        }
    }
}
