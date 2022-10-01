using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

class AmountConverter : JsonConverter<Amount>
{
    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Amount.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, Amount value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
