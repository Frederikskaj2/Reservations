using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class EmailAddressConverter : JsonConverter<EmailAddress>
{
    public override EmailAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => EmailAddress.FromString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, EmailAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
