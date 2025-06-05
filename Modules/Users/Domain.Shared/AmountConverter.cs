using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class AmountConverter : JsonConverter<Amount>
{
    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Amount.FromDecimal(reader.GetDecimal());

    public override void Write(Utf8JsonWriter writer, Amount value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToDecimal());
}
