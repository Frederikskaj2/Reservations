using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;
using static System.Linq.Expressions.Expression;
using Convert = System.Convert;

namespace Frederikskaj2.Reservations.Application;

class HashMapConverter : JsonConverterFactory
{
    static readonly MethodInfo enumParse = (from method in typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static)
        where method.Name is nameof(Enum.Parse)
        let parameters = method.GetParameters()
        where parameters.Length is 3 &&
              parameters[0].ParameterType == typeof(Type) &&
              parameters[1].ParameterType == typeof(string) &&
              parameters[2].ParameterType == typeof(bool)
        select method).Single();

    static readonly MethodInfo convertChangeType = (from method in typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
        where method.Name is nameof(Convert.ChangeType)
        let parameters = method.GetParameters()
        where parameters.Length is 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(Type)
        select method).Single();

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(HashMap<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArguments = typeToConvert.GetGenericArguments();
        var keyType = genericArguments[0];
        var valueType = genericArguments[1];
        var converterType = typeof(InnerHashMapConverter<,>).MakeGenericType(keyType, valueType);

        var toKey = keyType.IsEnum ? GetEnumDelegate(keyType) : GetConvertDelegate(keyType);
        return (JsonConverter?) Activator.CreateInstance(converterType, toKey);

        static Delegate GetEnumDelegate(Type type)
        {
            var parameter = Parameter(typeof(string));
            var call = Call(enumParse, Constant(type), parameter, Constant(true));
            var convert = Convert(call, type);
            var lambda = Lambda(convert, parameter);
            return lambda.Compile();
        }

        static Delegate GetConvertDelegate(Type type)
        {
            var parameter = Parameter(typeof(string));
            var call = Call(convertChangeType, parameter, Constant(type));
            var convert = Convert(call, type);
            var lambda = Lambda(convert, parameter);
            return lambda.Compile();
        }
    }

    class InnerHashMapConverter<TKey, TValue> : JsonConverter<HashMap<TKey, TValue>>
    {
        readonly Func<string, TKey> toKey;

        public InnerHashMapConverter(Func<string, TKey> toKey) => this.toKey = toKey;

        public override HashMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartObject)
                throw new JsonException();

            var tuples = new List<(TKey Key, TValue Value)>();

            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.EndObject)
                    return toHashMap(tuples);

                if (reader.TokenType is not JsonTokenType.PropertyName)
                    throw new JsonException();

                var key = toKey(reader.GetString()!);
                var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
                tuples.Add((key, value));
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, HashMap<TKey, TValue> hashMap, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, value) in hashMap)
            {
                writer.WritePropertyName(key!.ToString()!.ToCamelCase());
                JsonSerializer.Serialize(writer, value, options);
            }

            writer.WriteEndObject();
        }
    }
}
