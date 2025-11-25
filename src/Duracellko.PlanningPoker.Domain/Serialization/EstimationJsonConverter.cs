using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Duracellko.PlanningPoker.Domain.Serialization;

internal sealed class EstimationJsonConverter : JsonConverter<Estimation>
{
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
        else
        {
            writer.WriteNumberValue(value.Value.Value);
        }

        writer.WriteEndObject();
    }

    private static string GetPropertyName(string propertyName, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy != null ? options.PropertyNamingPolicy.ConvertName(propertyName) : propertyName;
    }

    private static bool IsPropertyName(ref Utf8JsonReader reader, string propertyName, JsonSerializerOptions options)
    {
        propertyName = GetPropertyName(propertyName, options);
        var stringComparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(reader.GetString(), propertyName, stringComparison);
    }

    [SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "Estimation value is only number, string or null.")]
    private static double? GetEstimationValue(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.Null => null,
            _ => throw new JsonException(),
        };
    }
}
