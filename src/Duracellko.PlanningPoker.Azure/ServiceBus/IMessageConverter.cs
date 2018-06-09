using Microsoft.Azure.ServiceBus;

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
        Message ConvertToBrokeredMessage(NodeMessage message);

        /// <summary>
        /// Converts BrokeredMessage message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        NodeMessage ConvertToNodeMessage(Message message);
    }
}
