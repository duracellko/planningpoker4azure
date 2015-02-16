// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationParticipantStatusTest
    {
        #region Constructor

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_MemberNameNotSpecified_ArgumentNullException()
        {
            // Arrange
            string name = null;

            // Act
            var result = new EstimationParticipantStatus(name, false);
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

        #endregion
    }
}
