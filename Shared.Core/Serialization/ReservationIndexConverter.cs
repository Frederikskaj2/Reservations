using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class ReservationIndexConverter : JsonConverter<ReservationIndex>
{
    public override ReservationIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ReservationIndex.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, ReservationIndex value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
