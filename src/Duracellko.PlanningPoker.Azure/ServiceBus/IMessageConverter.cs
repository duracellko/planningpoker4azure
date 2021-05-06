using Azure.Messaging.ServiceBus;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// When implemented, then object is able to convert messages of type <see cref="T:NodeMessage"/> to ServiceBusMessage and vice versa.
    /// </summary>
    public interface IMessageConverter
    {
        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to ServiceBusMessage  object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of ServiceBusMessage type.</returns>
        ServiceBusMessage ConvertToServiceBusMessage(NodeMessage message);

        /// <summary>
        /// Converts ServiceBusReceivedMessage message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        NodeMessage ConvertToNodeMessage(ServiceBusReceivedMessage message);
    }
}
