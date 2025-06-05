using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class TokenIdConverter : JsonConverter<TokenId>
{
    public override TokenId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TokenId.FromInt32(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, TokenId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.ToInt32());
}
