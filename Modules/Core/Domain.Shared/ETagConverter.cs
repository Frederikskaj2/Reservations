using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Core;

class ETagConverter : JsonConverter<ETag>
{
    public override ETag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, ETag value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
