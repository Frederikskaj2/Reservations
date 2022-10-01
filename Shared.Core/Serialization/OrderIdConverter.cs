using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class OrderIdConverter : JsonConverter<OrderId>
{
    public override OrderId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => OrderId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, OrderId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
