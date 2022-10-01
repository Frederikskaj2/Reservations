using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class PeriodConverter : JsonConverter<Period>
{
    static readonly PeriodPattern pattern = PeriodPattern.Roundtrip;

    public override Period? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var @string = reader.GetString();
        if (string.IsNullOrEmpty(@string))
            throw new JsonException();
        var result = pattern.Parse(@string);
        if (!result.Success)
            throw new JsonException("Cannot parse period.", result.Exception);
        return result.Value;
    }
    
    public override void Write(Utf8JsonWriter writer, Period value, JsonSerializerOptions options)
        => writer.WriteStringValue(pattern.Format(value));
}
