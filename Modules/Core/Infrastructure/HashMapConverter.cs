using LanguageExt;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

class HashMapConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(HashMap<,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InnerHashMapConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }

    class InnerHashMapConverter<TKey, TValue> : JsonConverter<HashMap<TKey, TValue>> where TKey : notnull
    {
        public override HashMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options);
            return toHashMap(dictionary.Map(kvp => (kvp.Key, kvp.Value)));
        }

        public override void Write(Utf8JsonWriter writer, HashMap<TKey, TValue> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value.ToDictionary(tuple => tuple.Key, tuple => tuple.Value), options);
    }
}
