// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// When implemented, then object is able to convert messages of type <see cref="T:NodeMessage"/> to BrokeredMessage and vice versa.
    /// </summary>
    public interface IMessageConverter
    {
        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to BrokeredMessage object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of BrokeredMessage type.</returns>
        BrokeredMessage ConvertToBrokeredMessage(NodeMessage message);

        /// <summary>
        /// Converts BrokeredMessage message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        NodeMessage ConvertToNodeMessage(BrokeredMessage message);
    }
}
