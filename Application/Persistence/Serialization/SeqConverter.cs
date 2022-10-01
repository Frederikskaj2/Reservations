using LanguageExt;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

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
        var genericArguments = typeToConvert.GetGenericArguments();
        var itemType = genericArguments[0];
        var converterType = typeof(InnerSeqConverter<>).MakeGenericType(itemType);
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }

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
}