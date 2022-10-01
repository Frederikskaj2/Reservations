using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class ResourceIdConverter : JsonConverter<ResourceId>
{
    public override ResourceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ResourceId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, ResourceId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
