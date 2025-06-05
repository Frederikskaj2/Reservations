using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Core;

class LocalDateConverter : JsonConverter<LocalDate>
{
    static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

    public override LocalDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var @string = reader.GetString();
        if (string.IsNullOrEmpty(@string))
            throw new JsonException();
        var result = pattern.Parse(@string);
        if (!result.Success)
            throw new JsonException("Cannot parse local date.", result.Exception);
        return result.Value;
    }

    public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
        => writer.WriteStringValue(pattern.Format(value));
}
