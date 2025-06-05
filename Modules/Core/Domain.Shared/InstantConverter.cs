using NodaTime;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Core;

class InstantConverter : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Instant.FromDateTimeUtc(reader.GetDateTime().ToUniversalTime());

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToDateTimeUtc());
}
