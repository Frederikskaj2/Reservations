using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Bank;

class BankTransactionIdConverter : JsonConverter<BankTransactionId>
{
    public override BankTransactionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => BankTransactionId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, BankTransactionId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
