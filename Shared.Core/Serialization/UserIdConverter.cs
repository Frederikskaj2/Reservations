using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class UserIdConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => UserId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
