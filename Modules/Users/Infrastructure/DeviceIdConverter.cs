using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class DeviceIdConverter : JsonConverter<DeviceId>
{
    public override DeviceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DeviceId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, DeviceId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
