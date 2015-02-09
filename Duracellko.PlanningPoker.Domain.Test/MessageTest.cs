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
    public class MessageTest
    {
        #region Constructor

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

        #endregion
    }
}
