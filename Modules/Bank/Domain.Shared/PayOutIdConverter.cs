using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Bank;

class PayOutIdConverter : JsonConverter<PayOutId>
{
    public override PayOutId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => PayOutId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, PayOutId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
