using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class TransactionIdConverter : JsonConverter<TransactionId>
{
    public override TransactionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TransactionId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, TransactionId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
