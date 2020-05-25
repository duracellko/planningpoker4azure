using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// Instance of this class is able to convert messages of type <see cref="T:NodeMessage"/> to BrokeredMessage and vice versa.
    /// </summary>
    public class MessageConverter : IMessageConverter
    {
        /// <summary>
        /// Name of property in BrokeredMessage holding recipient node ID.
        /// </summary>
        internal const string RecipientIdPropertyName = "RecipientId";

        /// <summary>
        /// Name of property in BrokeredMessage holding sender node ID.
        /// </summary>
        internal const string SenderIdPropertyName = "SenderId";

        private const string MessageTypePropertyName = "MessageType";
        private const string MessageSubtypePropertyName = "MessageSubtype";

        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to BrokeredMessage object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of BrokeredMessage type.</returns>
        public Message ConvertToBrokeredMessage(NodeMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] messageBody;
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
                messageBody = Array.Empty<byte>();
            }

            var result = new Message(messageBody);
            result.UserProperties[MessageTypePropertyName] = message.MessageType.ToString();
            if (message.Data != null)
            {
                result.UserProperties[MessageSubtypePropertyName] = message.Data.GetType().Name;
            }

            result.UserProperties[SenderIdPropertyName] = message.SenderNodeId;
            result.UserProperties[RecipientIdPropertyName] = message.RecipientNodeId;
            return result;
        }

        /// <summary>
        /// Converts BrokeredMessage message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        public NodeMessage ConvertToNodeMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = (NodeMessageType)Enum.Parse(typeof(NodeMessageType), (string)message.UserProperties[MessageTypePropertyName]);
            var messageSubtype = message.UserProperties.ContainsKey(MessageSubtypePropertyName) ? (string)message.UserProperties[MessageSubtypePropertyName] : null;

            var result = new NodeMessage(messageType);
            result.SenderNodeId = (string)message.UserProperties[SenderIdPropertyName];
            result.RecipientNodeId = (string)message.UserProperties[RecipientIdPropertyName];

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

        private static byte[] ConvertToMessageBody(object data)
        {
            void WriteData(object data, TextWriter writer)
            {
                var serializer = JsonSerializer.Create();
                serializer.Serialize(writer, data);
            }

            return ConvertToMessageBody(data, WriteData);
        }

        private static byte[] ConvertToMessageBody(string data)
        {
            void WriteData(string data, TextWriter writer)
            {
                writer.Write(data);
            }

            return ConvertToMessageBody(data, WriteData);
        }

        private static byte[] ConvertToMessageBody<T>(T data, Action<T, TextWriter> writeAction)
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

                return bodyStream.ToArray();
            }
        }

        private static T ConvertFromMessageBody<T>(byte[] body)
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

        private static string ConvertFromMessageBody(byte[] body)
        {
            string ReadData(TextReader reader)
            {
                return reader.ReadToEnd();
            }

            return ConvertFromMessageBody<string>(body, ReadData);
        }

        private static T ConvertFromMessageBody<T>(byte[] body, Func<TextReader, T> readFunction)
        {
            using (var bodyStream = new MemoryStream(body))
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
