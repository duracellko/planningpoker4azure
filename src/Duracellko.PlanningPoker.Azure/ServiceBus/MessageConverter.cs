using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
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

        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to ServiceBusMessage  object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of ServiceBusMessage type.</returns>
        public ServiceBusMessage ConvertToServiceBusMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            BinaryData messageBody;
            if (message.MessageType == NodeMessageType.InitializeTeam || message.MessageType == NodeMessageType.TeamCreated)
            {
                messageBody = ConvertToMessageBody((string)message.Data);
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
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = (NodeMessageType)Enum.Parse(typeof(NodeMessageType), (string)message.ApplicationProperties[MessageTypePropertyName]);
            string messageSubtype = null;
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
            void WriteData(object data, TextWriter writer)
            {
                var serializer = JsonSerializer.Create();
                serializer.Serialize(writer, data);
            }

            return ConvertToMessageBody(data, WriteData);
        }

        private static BinaryData ConvertToMessageBody(string data)
        {
            void WriteData(string data, TextWriter writer)
            {
                writer.Write(data);
            }

            return ConvertToMessageBody(data, WriteData);
        }

        private static BinaryData ConvertToMessageBody<T>(T data, Action<T, TextWriter> writeAction)
        {
            using (var bodyStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(bodyStream, CompressionMode.Compress, true))
                {
                    using (var writer = new StreamWriter(deflateStream, Encoding.UTF8))
                    {
                        writeAction(data, writer);
                        writer.Flush();
                        deflateStream.Flush();
                    }
                }

                return BinaryData.FromBytes(bodyStream.ToArray());
            }
        }

        private static T ConvertFromMessageBody<T>(BinaryData body)
        {
            T ReadData(TextReader reader)
            {
                var serializer = JsonSerializer.Create();
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }

            return ConvertFromMessageBody<T>(body, ReadData);
        }

        private static string ConvertFromMessageBody(BinaryData body)
        {
            string ReadData(TextReader reader)
            {
                return reader.ReadToEnd();
            }

            return ConvertFromMessageBody<string>(body, ReadData);
        }

        private static T ConvertFromMessageBody<T>(BinaryData body, Func<TextReader, T> readFunction)
        {
            using (var bodyStream = body.ToStream())
            {
                using (var deflateStream = new DeflateStream(bodyStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(deflateStream, Encoding.UTF8))
                    {
                        return readFunction(reader);
                    }
                }
            }
        }
    }
}
