using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;

namespace Duracellko.PlanningPoker.Azure.ServiceBus;

/// <summary>
/// Instance of this class is able to convert messages of type <see cref="T:NodeMessage"/> to ServiceBusMessage and vice versa.
/// </summary>
public class MessageConverter : IMessageConverter
{
    /// <summary>
    /// Name of property in ServiceBusMessage holding recipient node ID.
    /// </summary>
    internal const string RecipientIdPropertyName = "RecipientId";

    /// <summary>
    /// Name of property in ServiceBusMessage holding sender node ID.
    /// </summary>
    internal const string SenderIdPropertyName = "SenderId";

    private const string MessageTypePropertyName = "MessageType";
    private const string MessageSubtypePropertyName = "MessageSubtype";

    private static readonly BinaryData _emptyBinaryData = new BinaryData(Array.Empty<byte>());
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    /// <summary>
    /// Converts <see cref="T:NodeMessage"/> message to ServiceBusMessage  object.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Converted message of ServiceBusMessage type.</returns>
    public ServiceBusMessage ConvertToServiceBusMessage(NodeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        BinaryData messageBody;
        if (message.MessageType == NodeMessageType.InitializeTeam || message.MessageType == NodeMessageType.TeamCreated)
        {
            messageBody = ConvertToMessageBody((byte[])message.Data!);
        }
        else if (message.Data != null)
        {
            messageBody = ConvertToMessageBody(message.Data);
        }
        else
        {
            messageBody = _emptyBinaryData;
        }

        var result = new ServiceBusMessage(messageBody);
        result.ApplicationProperties[MessageTypePropertyName] = message.MessageType.ToString();
        if (message.Data != null)
        {
            result.ApplicationProperties[MessageSubtypePropertyName] = message.Data.GetType().Name;
        }

        result.ApplicationProperties[SenderIdPropertyName] = message.SenderNodeId;
        result.ApplicationProperties[RecipientIdPropertyName] = message.RecipientNodeId;
        return result;
    }

    /// <summary>
    /// Converts ServiceBusReceivedMessage message to <see cref="T:NodeMessage"/> object.
    /// </summary>
    /// <param name="message">The message to convert.</param>
    /// <returns>Converted message of NodeMessage type.</returns>
    public NodeMessage ConvertToNodeMessage(ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = (NodeMessageType)Enum.Parse(typeof(NodeMessageType), (string)message.ApplicationProperties[MessageTypePropertyName]);
        string? messageSubtype = null;
        if (message.ApplicationProperties.TryGetValue(MessageSubtypePropertyName, out var messageSubtypeObject))
        {
            messageSubtype = (string)messageSubtypeObject;
        }

        var result = new NodeMessage(messageType);
        result.SenderNodeId = (string)message.ApplicationProperties[SenderIdPropertyName];
        result.RecipientNodeId = (string)message.ApplicationProperties[RecipientIdPropertyName];

        switch (result.MessageType)
        {
            case NodeMessageType.ScrumTeamMessage:
                if (string.Equals(messageSubtype, typeof(ScrumTeamMemberMessage).Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMemberMessage>(message.Body);
                }
                else if (string.Equals(messageSubtype, typeof(ScrumTeamMemberEstimationMessage).Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMemberEstimationMessage>(message.Body);
                }
                else if (string.Equals(messageSubtype, typeof(ScrumTeamEstimationSetMessage).Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamEstimationSetMessage>(message.Body);
                }
                else if (string.Equals(messageSubtype, typeof(ScrumTeamTimerMessage).Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamTimerMessage>(message.Body);
                }
                else
                {
                    result.Data = ConvertFromMessageBody<ScrumTeamMessage>(message.Body);
                }

                break;
            case NodeMessageType.TeamCreated:
            case NodeMessageType.InitializeTeam:
                result.Data = ConvertFromMessageBody(message.Body);
                break;
            case NodeMessageType.TeamList:
            case NodeMessageType.RequestTeams:
                result.Data = ConvertFromMessageBody<string[]>(message.Body);
                break;
        }

        return result;
    }

    private static BinaryData ConvertToMessageBody(object data)
    {
        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(data, data.GetType(), _jsonSerializerOptions);
        return BinaryData.FromBytes(dataBytes);
    }

    private static BinaryData ConvertToMessageBody(byte[] data)
    {
        using (var dataStream = new MemoryStream())
        {
            using (var deflateStream = new DeflateStream(dataStream, CompressionMode.Compress, true))
            {
                deflateStream.Write(data, 0, data.Length);
                deflateStream.Flush();
            }

            return BinaryData.FromBytes(dataStream.ToArray());
        }
    }

    private static T? ConvertFromMessageBody<T>(BinaryData body)
    {
        return JsonSerializer.Deserialize<T>(body, _jsonSerializerOptions);
    }

    private static byte[] ConvertFromMessageBody(BinaryData body)
    {
        using (var dataStream = body.ToStream())
        {
            using (var deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress))
            {
                using var memoryStream = new MemoryStream();
                deflateStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
