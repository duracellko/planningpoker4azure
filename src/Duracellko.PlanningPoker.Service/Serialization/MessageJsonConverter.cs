using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Duracellko.PlanningPoker.Service.Serialization
{
    /// <summary>
    /// Converts a Message object to and from JSON.
    /// </summary>
    public class MessageJsonConverter : JsonConverter<Message>
    {
        /// <summary>
        /// Determines whether the specified type can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type to compare against.</param>
        /// <returns><c>true</c> if the type can be converted; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Message).IsAssignableFrom(typeToConvert);
        }

        /// <summary>
        /// Reads and converts the JSON to type T.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var messageData = default(MessageData);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return CreateMessageFromData(ref messageData);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                ReadMessageDataProperty(ref reader, ref messageData, options);
            }

            throw new JsonException();
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            writer.WriteStartObject();

            writer.WritePropertyName(GetPropertyName(nameof(Message.Id), options));
            writer.WriteNumberValue(value.Id);

            writer.WritePropertyName(GetPropertyName(nameof(Message.Type), options));
            JsonSerializer.Serialize(writer, value.Type, options);

            if (value is MemberMessage memberMessage)
            {
                writer.WritePropertyName(GetPropertyName(nameof(MemberMessage.Member), options));
                JsonSerializer.Serialize(writer, memberMessage.Member, options);
            }
            else if (value is EstimationResultMessage estimationResultMessage)
            {
                writer.WritePropertyName(GetPropertyName(nameof(EstimationResultMessage.EstimationResult), options));
                JsonSerializer.Serialize(writer, estimationResultMessage.EstimationResult, options);
            }
            else if (value is EstimationSetMessage estimationSetMessage)
            {
                writer.WritePropertyName(GetPropertyName(nameof(EstimationSetMessage.Estimations), options));
                JsonSerializer.Serialize(writer, estimationSetMessage.Estimations, options);
            }
            else if (value is TimerMessage timerMessage)
            {
                writer.WritePropertyName(GetPropertyName(nameof(TimerMessage.EndTime), options));
                JsonSerializer.Serialize(writer, timerMessage.EndTime, options);
            }

            writer.WriteEndObject();
        }

        private static string GetPropertyName(string propertyName, JsonSerializerOptions options)
        {
            if (options.PropertyNamingPolicy != null)
            {
                return options.PropertyNamingPolicy.ConvertName(propertyName);
            }

            return propertyName;
        }

        private static bool IsPropertyName(ref Utf8JsonReader reader, string propertyName, JsonSerializerOptions options)
        {
            propertyName = GetPropertyName(propertyName, options);
            var stringComparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(reader.GetString(), propertyName, stringComparison);
        }

        private static Message CreateMessageFromData(ref MessageData messageData)
        {
            Message message;
            switch (messageData.Type)
            {
                case MessageType.MemberJoined:
                case MessageType.MemberDisconnected:
                case MessageType.MemberEstimated:
                    message = new MemberMessage
                    {
                        Member = messageData.Member
                    };
                    break;
                case MessageType.EstimationEnded:
                    var estimationResultMessage = new EstimationResultMessage();
                    if (messageData.EstimationResult != null)
                    {
                        estimationResultMessage.EstimationResult = messageData.EstimationResult;
                    }

                    message = estimationResultMessage;
                    break;
                case MessageType.AvailableEstimationsChanged:
                    var estimationSetMessage = new EstimationSetMessage();
                    if (messageData.Estimations != null)
                    {
                        estimationSetMessage.Estimations = messageData.Estimations;
                    }

                    message = estimationSetMessage;
                    break;
                case MessageType.TimerStarted:
                    message = new TimerMessage
                    {
                        EndTime = DateTime.SpecifyKind(messageData.EndTimer, DateTimeKind.Utc)
                    };
                    break;
                default:
                    message = new Message();
                    break;
            }

            message.Id = messageData.Id;
            message.Type = messageData.Type;
            return message;
        }

        private static void ReadMessageDataProperty(ref Utf8JsonReader reader, ref MessageData messageData, JsonSerializerOptions options)
        {
            if (IsPropertyName(ref reader, nameof(Message.Id), options))
            {
                messageData.Id = JsonSerializer.Deserialize<long>(ref reader, options);
            }
            else if (IsPropertyName(ref reader, nameof(Message.Type), options))
            {
                messageData.Type = JsonSerializer.Deserialize<MessageType>(ref reader, options);
            }
            else if (IsPropertyName(ref reader, nameof(MemberMessage.Member), options))
            {
                messageData.Member = JsonSerializer.Deserialize<TeamMember>(ref reader, options);
            }
            else if (IsPropertyName(ref reader, nameof(EstimationResultMessage.EstimationResult), options))
            {
                messageData.EstimationResult = JsonSerializer.Deserialize<IList<EstimationResultItem>>(ref reader, options);
            }
            else if (IsPropertyName(ref reader, nameof(EstimationSetMessage.Estimations), options))
            {
                messageData.Estimations = JsonSerializer.Deserialize<IList<Estimation>>(ref reader, options);
            }
            else if (IsPropertyName(ref reader, nameof(TimerMessage.EndTime), options))
            {
                messageData.EndTimer = JsonSerializer.Deserialize<DateTime>(ref reader, options);
            }
            else
            {
                reader.Skip();
            }
        }

        private ref struct MessageData
        {
            public long Id { get; set; }

            public MessageType Type { get; set; }

            public TeamMember? Member { get; set; }

            public IList<EstimationResultItem>? EstimationResult { get; set; }

            public IList<Estimation>? Estimations { get; set; }

            public DateTime EndTimer { get; set; }
        }
    }
}
