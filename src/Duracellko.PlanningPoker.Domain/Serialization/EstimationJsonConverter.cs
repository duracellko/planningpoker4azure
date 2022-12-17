using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    internal sealed class EstimationJsonConverter : JsonConverter<Estimation>
    {
        private const string PositiveInfinity = "Infinity";

        public override Estimation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            double? estimationValue = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Estimation(estimationValue);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (IsPropertyName(ref reader, nameof(Estimation.Value), options))
                {
                    if (!reader.Read())
                    {
                        throw new JsonException();
                    }

                    estimationValue = GetEstimationValue(ref reader);
                }
                else
                {
                    reader.Skip();
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Estimation value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(GetPropertyName(nameof(Estimation.Value), options));
            if (!value.Value.HasValue)
            {
                writer.WriteNullValue();
            }
            else if (double.IsPositiveInfinity(value.Value.Value))
            {
                writer.WriteStringValue(PositiveInfinity);
            }
            else
            {
                writer.WriteNumberValue(value.Value.Value);
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

        private static double? GetEstimationValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetDouble();
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    var estimationStringValue = reader.GetString();
                    if (!string.Equals(estimationStringValue, PositiveInfinity, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new JsonException();
                    }

                    return double.PositiveInfinity;
                default:
                    throw new JsonException();
            }
        }
    }
}
