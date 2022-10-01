using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class ReservationIdConverter : JsonConverter<ReservationId>
{
    public override ReservationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var @string = reader.GetString();
        if (string.IsNullOrEmpty(@string) || !ReservationId.TryParse(@string, out var reservationId))
            throw new JsonException();
        return reservationId;
    }

    public override void Write(Utf8JsonWriter writer, ReservationId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
