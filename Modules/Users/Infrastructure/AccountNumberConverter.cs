using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class AccountNumberConverter : JsonConverter<AccountNumber>
{
    public override AccountNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => AccountNumber.FromString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, AccountNumber value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
