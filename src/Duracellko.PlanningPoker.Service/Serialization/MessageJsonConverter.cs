using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Duracellko.PlanningPoker.Service.Serialization
{
    /// <summary>
    /// Converts a Message object to and from JSON.
    /// </summary>
    public class MessageJsonConverter : JsonConverter
    {
        /// <summary>
        /// Gets a value indicating whether this JsonConverter can write JSON.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Message);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            var jsonObject = JObject.Load(reader);
            var messageTypeValue = (JValue)jsonObject.GetValue(nameof(Message.Type), StringComparison.OrdinalIgnoreCase);
            var messageType = (MessageType)Convert.ToInt32(messageTypeValue.Value, CultureInfo.InvariantCulture);

            Message message;
            switch (messageType)
            {
                case MessageType.MemberJoined:
                case MessageType.MemberDisconnected:
                case MessageType.MemberEstimated:
                    message = GetMemberMessage(jsonObject, serializer);
                    break;
                case MessageType.EstimationEnded:
                    message = GetEstimationResultMessage(jsonObject, serializer);
                    break;
                default:
                    message = new Message();
                    break;
            }

            SetMessageProperties(message, jsonObject, messageType);
            return message;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private static void SetMessageProperties(Message message, JObject jsonObject, MessageType messageType)
        {
            var idValue = (JValue)jsonObject.GetValue(nameof(Message.Id), StringComparison.OrdinalIgnoreCase);
            message.Id = Convert.ToInt64(idValue.Value, CultureInfo.InvariantCulture);
            message.Type = messageType;
        }

        private static MemberMessage GetMemberMessage(JObject jsonObject, JsonSerializer serializer)
        {
            var memberValue = jsonObject.GetValue(nameof(MemberMessage.Member), StringComparison.OrdinalIgnoreCase);
            return new MemberMessage
            {
                Member = GetObjectFromJToken<TeamMember>(memberValue, serializer)
            };
        }

        private static EstimationResultMessage GetEstimationResultMessage(JObject jsonObject, JsonSerializer serializer)
        {
            var estimationResultValue = jsonObject.GetValue(nameof(EstimationResultMessage.EstimationResult), StringComparison.OrdinalIgnoreCase);
            return new EstimationResultMessage
            {
                EstimationResult = GetObjectFromJToken<IList<EstimationResultItem>>(estimationResultValue, serializer)
            };
        }

        private static T GetObjectFromJToken<T>(JToken token, JsonSerializer serializer)
        {
            if (token == null)
            {
                return default;
            }

            using (var reader = new JTokenReader(token))
            {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
