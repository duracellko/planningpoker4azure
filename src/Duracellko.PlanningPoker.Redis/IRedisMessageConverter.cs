using Duracellko.PlanningPoker.Azure;
using StackExchange.Redis;

namespace Duracellko.PlanningPoker.Redis;

/// <summary>
/// When implemented, then object is able to convert messages of type <see cref="T:NodeMessage"/> to RedisValue and vice versa.
/// </summary>
public interface IRedisMessageConverter
{
    /// <summary>
    /// Converts <see cref="T:NodeMessage"/> message to RedisValue object.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Converted message of RedisValue type.</returns>
    RedisValue ConvertToRedisMessage(NodeMessage message);

    /// <summary>
    /// Converts RedisValue message to <see cref="T:NodeMessage"/> object.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Converted message of NodeMessage type.</returns>
    NodeMessage ConvertToNodeMessage(RedisValue message);

    /// <summary>
    /// Gets message header informarion: sender, recipient and message type.
    /// </summary>
    /// <param name="message">The message to read headers from.</param>
    /// <returns>NodeMessage object that contains header information, but no data.</returns>
    NodeMessage GetMessageHeader(RedisValue message);
}
