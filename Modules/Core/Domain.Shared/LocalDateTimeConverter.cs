using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Core;

class LocalDateTimeConverter : JsonConverter<LocalDateTime>
{
    static readonly LocalDateTimePattern pattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-ddTHH:mm:ss");

    public override LocalDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var @string = reader.GetString();
        if (string.IsNullOrEmpty(@string))
            throw new JsonException();
        var result = pattern.Parse(@string);
        if (!result.Success)
            throw new JsonException("Cannot parse local date.", result.Exception);
        return result.Value;
    }

    public override void Write(Utf8JsonWriter writer, LocalDateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(pattern.Format(value));
}
