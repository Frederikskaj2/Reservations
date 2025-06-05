using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class ApartmentIdConverter : JsonConverter<ApartmentId>
{
    public override ApartmentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ApartmentId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, ApartmentId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
