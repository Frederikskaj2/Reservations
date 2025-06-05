using LanguageExt;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

class SeqConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Seq<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(Seq<byte>))
            return new SeqByteConverter();
        var converterType = typeof(InnerSeqConverter<>).MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }

    // This serializer avoids relying on serialization of arrays complicating it a
    // bit, but it's done to reduce the allocations needed.
    class InnerSeqConverter<T> : JsonConverter<Seq<T>>
    {
        public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            Seq<T> seq = Empty;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return seq;
                var value = JsonSerializer.Deserialize<T>(ref reader, options)!;
                seq = seq.Add(value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }

    class SeqByteConverter : JsonConverter<Seq<byte>>
    {
        public override Seq<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value is not null ? Convert.FromBase64String(value).ToSeq() : default;
        }

        public override void Write(Utf8JsonWriter writer, Seq<byte> value, JsonSerializerOptions options)
        {
            var bytes = value.ToArray();
            writer.WriteStringValue(Convert.ToBase64String(bytes));
        }
    }
}
