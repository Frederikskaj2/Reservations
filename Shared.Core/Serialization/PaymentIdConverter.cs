using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class PaymentIdConverter : JsonConverter<PaymentId>
{
    public override PaymentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        PaymentId.FromString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, PaymentId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
