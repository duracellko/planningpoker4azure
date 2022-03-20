using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Duracellko.PlanningPoker.Azure;
using StackExchange.Redis;

namespace Duracellko.PlanningPoker.Redis
{
    /// <summary>
    /// Instance of this class is able to convert messages of type <see cref="T:NodeMessage"/> to RedisValue and vice versa.
    /// </summary>
    public class RedisMessageConverter : IRedisMessageConverter
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to RedisValue object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of RedisValue type.</returns>
        public RedisValue ConvertToRedisMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using var dataStream = new MemoryStream();
            WriteStringToStream(message.SenderNodeId, dataStream);
            WriteStringToStream(message.RecipientNodeId, dataStream);
            dataStream.WriteByte((byte)message.MessageType);
            WriteStringToStream(message.Data?.GetType().Name, dataStream);
            dataStream.Flush();

            if (message.MessageType == NodeMessageType.InitializeTeam || message.MessageType == NodeMessageType.TeamCreated)
            {
                WriteToStream((byte[])message.Data!, dataStream);
            }
            else if (message.Data != null)
            {
                WriteToStream(message.Data, dataStream);
            }

            return RedisValue.CreateFrom(dataStream);
        }

        /// <summary>
        /// Converts RedisValue message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        public NodeMessage ConvertToNodeMessage(RedisValue message)
        {
            if (message.IsNullOrEmpty)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var data = ((ReadOnlyMemory<byte>)message).Span;
            data = ReadString(data, out var senderNodeId);
            data = ReadString(data, out var recipientNodeId);

            var messageType = (NodeMessageType)data[0];
            data = data.Slice(1);
            if (!Enum.IsDefined<NodeMessageType>(messageType))
            {
                throw new ArgumentException("Invalid message format.", nameof(message));
            }

            data = ReadString(data, out var messageSubtype);

            var result = new NodeMessage(messageType);
            result.SenderNodeId = senderNodeId;
            result.RecipientNodeId = recipientNodeId;

            switch (result.MessageType)
            {
                case NodeMessageType.ScrumTeamMessage:
                    if (string.Equals(messageSubtype, typeof(ScrumTeamMemberMessage).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Data = ReadObject<ScrumTeamMemberMessage>(data);
                    }
                    else if (string.Equals(messageSubtype, typeof(ScrumTeamMemberEstimationMessage).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Data = ReadObject<ScrumTeamMemberEstimationMessage>(data);
                    }
                    else if (string.Equals(messageSubtype, typeof(ScrumTeamTimerMessage).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Data = ReadObject<ScrumTeamTimerMessage>(data);
                    }
                    else
                    {
                        result.Data = ReadObject<ScrumTeamMessage>(data);
                    }

                    break;
                case NodeMessageType.TeamCreated:
                case NodeMessageType.InitializeTeam:
                    result.Data = ReadBinary(data);
                    break;
                case NodeMessageType.TeamList:
                case NodeMessageType.RequestTeams:
                    result.Data = ReadObject<string[]>(data);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets message header informarion: sender, recipient and message type.
        /// </summary>
        /// <param name="message">The message to read headers from.</param>
        /// <returns>NodeMessage object that contains header information, but no data.</returns>
        public NodeMessage GetMessageHeader(RedisValue message)
        {
            if (message.IsNullOrEmpty)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var data = ((ReadOnlyMemory<byte>)message).Span;
            data = ReadString(data, out var senderNodeId);
            data = ReadString(data, out var recipientNodeId);
            var messageType = (NodeMessageType)data[0];

            if (!Enum.IsDefined<NodeMessageType>(messageType))
            {
                throw new ArgumentException("Invalid message format.", nameof(message));
            }

            return new NodeMessage(messageType)
            {
                SenderNodeId = senderNodeId,
                RecipientNodeId = recipientNodeId
            };
        }

        private static void WriteStringToStream(string? value, Stream stream)
        {
            if (value == null)
            {
                stream.WriteByte(255);
            }
            else if (value.Length == 0)
            {
                stream.WriteByte(0);
            }
            else
            {
                var data = Encoding.UTF8.GetBytes(value);
                stream.WriteByte((byte)data.Length);
                stream.Write(data);
            }
        }

        private static void WriteToStream(object data, Stream stream)
        {
            JsonSerializer.Serialize(stream, data, data.GetType(), _jsonSerializerOptions);
        }

        private static void WriteToStream(byte[] data, Stream stream)
        {
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Compress, true))
            {
                deflateStream.Write(data);
                deflateStream.Flush();
            }
        }

        private static ReadOnlySpan<byte> ReadString(ReadOnlySpan<byte> data, out string? value)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentException("Invalid message format.", nameof(data));
            }

            var length = data[0];
            if (length == 255)
            {
                value = null;
                return data.Slice(1);
            }
            else if (length == 0)
            {
                value = string.Empty;
                return data.Slice(1);
            }
            else
            {
                value = Encoding.UTF8.GetString(data.Slice(1, length));
                return data.Slice(length + 1);
            }
        }

        private static T? ReadObject<T>(ReadOnlySpan<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
        }

        private static byte[] ReadBinary(ReadOnlySpan<byte> data)
        {
            using (var dataStream = new MemoryStream(data.ToArray()))
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
}
