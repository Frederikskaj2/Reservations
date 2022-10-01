using LanguageExt;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Application;

class SeqByteConverter : JsonConverter<Seq<byte>>
{
    public override Seq<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return default;
        return Convert.FromBase64String(value).ToSeq();
    }

    public override void Write(Utf8JsonWriter writer, Seq<byte> value, JsonSerializerOptions options)
    {
        var bytes = value.ToArray();
        writer.WriteStringValue(Convert.ToBase64String(bytes));
    }
}