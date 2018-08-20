using System;
using System.Threading.Tasks;

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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendMessage(NodeMessage message);

        /// <summary>
        /// Register for receiving messages from other nodes.
        /// </summary>
        /// <param name="nodeId">Current node ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Register(string nodeId);

        /// <summary>
        /// Stop receiving messages from other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Unregister();
    }
}
