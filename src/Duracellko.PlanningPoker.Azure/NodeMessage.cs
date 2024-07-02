namespace Duracellko.PlanningPoker.Azure;

/// <summary>
/// Message sent from one Planning Poker Azure node to another or broadcasted to all nodes.
/// </summary>
public class NodeMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeMessage"/> class.
    /// </summary>
    /// <param name="messageType">The node message type.</param>
    public NodeMessage(NodeMessageType messageType)
    {
        MessageType = messageType;
    }

    /// <summary>
    /// Gets a type of message.
    /// </summary>
    /// <value>The message type.</value>
    public NodeMessageType MessageType { get; private set; }

    /// <summary>
    /// Gets or sets an ID of sender.
    /// </summary>
    /// <value>The sender ID.</value>
    public string? SenderNodeId { get; set; }

    /// <summary>
    /// Gets or sets an ID of recipient or null, if message is broadcasted to all nodes.
    /// </summary>
    /// <value>The recipient ID.</value>
    public string? RecipientNodeId { get; set; }

    /// <summary>
    /// Gets or sets a message data.
    /// </summary>
    /// <value>The message data.</value>
    public object? Data { get; set; }
}
