// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// When implemented, then object is able to send and receive messages via service bus.
    /// </summary>
    public interface IServiceBus
    {
        /// <summary>
        /// Gets an observable object receiving messages from service bus.
        /// </summary>
        IObservable<NodeMessage> ObservableMessages { get; }

        /// <summary>
        /// Sends a message to service bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendMessage(NodeMessage message);

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        void Register(string nodeId);

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        void Unregister();
    }
}
