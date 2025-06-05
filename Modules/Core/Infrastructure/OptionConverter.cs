using LanguageExt;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

class OptionConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Option<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InnerOptionConverter<>).MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }

    class InnerOptionConverter<T> : JsonConverter<Option<T>>
    {
        public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType is not JsonTokenType.Null
                ? Some(JsonSerializer.Deserialize<T>(ref reader, options)!)
                : None;

        public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) =>
            value.Match(
                t => JsonSerializer.Serialize(writer, t, options),
                // When OptionAwareJsonTypeInfoResolver is used this is never written.
                writer.WriteNullValue);
    }
}
