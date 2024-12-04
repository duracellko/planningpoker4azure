using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Duracellko.PlanningPoker.Azure;

namespace Duracellko.PlanningPoker.RabbitMQ;

/// <summary>
/// Instance of this class is able to convert messages of type <see cref="T:NodeMessage"/> to RabbitMQ message and vice versa.
/// </summary>
public class MessageConverter : IMessageConverter
{
    /// <summary>
    /// Name of property in Rabbit MQ message holding recipient node ID.
    /// </summary>
    internal const string RecipientIdPropertyName = PropertyPrefix + "RecipientId";

    /// <summary>
    /// Name of property in Rabbit MQ message holding sender node ID.
    /// </summary>
    internal const string SenderIdPropertyName = PropertyPrefix + "SenderId";

    private const string PropertyPrefix = "PlanningPoker-";
    private const string MessageTypePropertyName = PropertyPrefix + "MessageType";
    private const string MessageSubtypePropertyName = PropertyPrefix + "MessageSubtype";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private static readonly Encoding _headersEncoding = Encoding.UTF8;

    /// <summary>
    /// Gets headers of RabbitMQ message converted from <see cref="T:NodeMessage"/>.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Headers of the message.</returns>
    public IDictionary<string, object?> GetMessageHeaders(NodeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var headers = new Dictionary<string, object?>();
        SetHeader(headers, MessageTypePropertyName, message.MessageType.ToString());
        if (message.Data != null)
        {
            SetHeader(headers, MessageSubtypePropertyName, message.Data.GetType().Name);
        }

        SetHeader(headers, SenderIdPropertyName, message.SenderNodeId);
        SetHeader(headers, RecipientIdPropertyName, message.RecipientNodeId);

        return headers;
    }

    /// <summary>
    /// Gets body of RabbitMQ message converted from <see cref="T:NodeMessage"/>.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Body of the message.</returns>
    public ReadOnlyMemory<byte> GetMessageBody(NodeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.MessageType is NodeMessageType.InitializeTeam or NodeMessageType.TeamCreated)
        {
            return ConvertToMessageBody((byte[])message.Data!);
        }
        else if (message.Data != null)
        {
            return ConvertToMessageBody(message.Data);
        }
        else
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Converts RabbitMQ message headers and body to <see cref="T:NodeMessage"/> object.
    /// </summary>
    /// <param name="headers">Headers of the message to convert.</param>
    /// <param name="body">Body of the message to convert.</param>
    /// <returns>Converted message of NodeMessage type.</returns>
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Condition has multiple branches.")]
    public NodeMessage GetNodeMessage(IDictionary<string, object?> headers, ReadOnlyMemory<byte> body)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var messageTypeValue = GetHeader(headers, MessageTypePropertyName);
        var messageType = (NodeMessageType)Enum.Parse(typeof(NodeMessageType), messageTypeValue!);
        var messageSubtype = GetHeader(headers, MessageSubtypePropertyName);

        var result = new NodeMessage(messageType)
        {
            SenderNodeId = GetHeader(headers, SenderIdPropertyName),
            RecipientNodeId = GetHeader(headers, RecipientIdPropertyName)
        };

        switch (result.MessageType)
        {
            case NodeMessageType.ScrumTeamMessage:
                if (string.Equals(messageSubtype, nameof(ScrumTeamMemberMessage), StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMemberMessage>(body);
                }
                else if (string.Equals(messageSubtype, nameof(ScrumTeamMemberEstimationMessage), StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMemberEstimationMessage>(body);
                }
                else if (string.Equals(messageSubtype, nameof(ScrumTeamEstimationSetMessage), StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamEstimationSetMessage>(body);
                }
                else if (string.Equals(messageSubtype, nameof(ScrumTeamTimerMessage), StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamTimerMessage>(body);
                }
                else
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMessage>(body);
                }

                break;
            case NodeMessageType.TeamCreated:
            case NodeMessageType.InitializeTeam:
                result.Data = ConvertFromMessageBody(body);
                break;
            case NodeMessageType.TeamList:
            case NodeMessageType.RequestTeams:
                result.Data = ConvertFromMessageBody<string[]>(body);
                break;
            case NodeMessageType.RequestTeamList:
            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// Gets decoded value of Rabbit MQ message header with specified key.
    /// </summary>
    /// <param name="headers">The collection of header key-value pairs.</param>
    /// <param name="key">The key to get header value for.</param>
    /// <returns>Value header with specified key.</returns>
    public string? GetHeader(IDictionary<string, object?> headers, string key)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNullOrEmpty(key);

        if (headers.TryGetValue(key, out var valueObject) && valueObject != null)
        {
            return _headersEncoding.GetString((byte[])valueObject);
        }

        return null;
    }

    private static ReadOnlyMemory<byte> ConvertToMessageBody(object data)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, data.GetType(), _jsonSerializerOptions);
    }

    private static ReadOnlyMemory<byte> ConvertToMessageBody(byte[] data)
    {
        using var dataStream = new MemoryStream();
        using (var deflateStream = new DeflateStream(dataStream, CompressionMode.Compress, true))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Flush();
        }

        return dataStream.ToArray();
    }

    private static T? ConvertFromMessageBody<T>(ReadOnlyMemory<byte> body) => JsonSerializer.Deserialize<T>(body.Span, _jsonSerializerOptions);

    private static byte[] ConvertFromMessageBody(ReadOnlyMemory<byte> body)
    {
        using var dataStream = new MemoryStream(body.ToArray());
        using var deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress);
        using var memoryStream = new MemoryStream();
        deflateStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static void SetHeader(Dictionary<string, object?> headers, string key, string? value)
    {
        if (value != null)
        {
            headers[key] = _headersEncoding.GetBytes(value);
        }
    }
}
