using NodaTime;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class LocalTimeConverter : JsonConverter<LocalTime>
{
    public override LocalTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var @string = reader.GetString();
        if (string.IsNullOrEmpty(@string) || !TimeSpan.TryParse(@string, CultureInfo.InvariantCulture, out var timeSpan))
            throw new JsonException();
        return LocalTime.FromTicksSinceMidnight(timeSpan.Ticks);
    }

    public override void Write(Utf8JsonWriter writer, LocalTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(TimeSpan.FromTicks(value.TickOfDay).ToString());
}
