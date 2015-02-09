// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationResultMessageTest
    {
        #region Constructor

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

        #endregion

        #region Constructor

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

        #endregion
    }
}
