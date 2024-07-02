using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Object implements sending messages to client.
/// </summary>
public interface IPlanningPokerClient
{
    /// <summary>
    /// Notifies client that there are new messages.
    /// </summary>
    /// <param name="messages">Collection of messages for specific client.</param>
    /// <returns>Asynchronous opperation.</returns>
    Task Notify(IList<Message> messages);
}
