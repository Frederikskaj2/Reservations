using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Orders;

class EntryCodeConverter : JsonConverter<EntryCode>
{
    public override EntryCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        EntryCode.FromString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, EntryCode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
