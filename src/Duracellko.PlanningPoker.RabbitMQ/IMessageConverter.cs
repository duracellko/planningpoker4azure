using System;
using System.Collections.Generic;
using Duracellko.PlanningPoker.Azure;

namespace Duracellko.PlanningPoker.RabbitMQ;

/// <summary>
/// When implemented, then object is able to convert messages of type <see cref="T:NodeMessage"/> to RabbitMQ message and vice versa.
/// </summary>
public interface IMessageConverter
{
    /// <summary>
    /// Gets headers of RabbitMQ message converted from <see cref="T:NodeMessage"/>.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Headers of the message.</returns>
    IDictionary<string, object> GetMessageHeaders(NodeMessage message);

    /// <summary>
    /// Gets body of RabbitMQ message converted from <see cref="T:NodeMessage"/>.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Body of the message.</returns>
    ReadOnlyMemory<byte> GetMessageBody(NodeMessage message);

    /// <summary>
    /// Converts RabbitMQ message headers and body to <see cref="T:NodeMessage"/> object.
    /// </summary>
    /// <param name="headers">Headers of the message to convert.</param>
    /// <param name="body">Body of the message to convert.</param>
    /// <returns>Converted message of NodeMessage type.</returns>
    NodeMessage GetNodeMessage(IDictionary<string, object> headers, ReadOnlyMemory<byte> body);

    /// <summary>
    /// Gets decoded value of Rabbit MQ message header with specified key.
    /// </summary>
    /// <param name="headers">The collection of header key-value pairs.</param>
    /// <param name="key">The key to get header value for.</param>
    /// <returns>Value header with specified key.</returns>
    string? GetHeader(IDictionary<string, object> headers, string key);
}
