using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationParticipantStatusTest
    {
        [TestMethod]
        public void Constructor_MemberNameSpecified_MemberNameIsSet()
        {
            // Arrange
            var name = "Member";

            // Act
            var result = new EstimationParticipantStatus(name, false);

            // Verify
            Assert.AreEqual<string>(name, result.MemberName);
        }

        [TestMethod]
        public void Constructor_MemberNameNotSpecified_ArgumentNullException()
        {
            // Arrange
            string name = null;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new EstimationParticipantStatus(name, false));
        }

        [TestMethod]
        public void Constructor_EstimatedSpecified_EstimatedIsSet()
        {
            // Arrange
            var estimated = true;

            // Act
            var result = new EstimationParticipantStatus("Member", estimated);

            // Verify
            Assert.IsTrue(estimated);
        }
    }
}
