using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Linq.Expressions.Expression;

namespace Frederikskaj2.Reservations.Shared.Core;

class IntKeyDictionaryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!IsGenericDictionaryType(typeToConvert))
            return false;
        var keyType = typeToConvert.GetGenericArguments()[0];
        return keyType.IsEnum || IsInt32Convertible(keyType);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArguments = typeToConvert.GetGenericArguments();
        var keyType = genericArguments[0];
        var valueType = genericArguments[1];
        var converterType = typeof(DictionaryEnumConverterInner<,>).MakeGenericType(keyType, valueType);

        if (keyType.IsEnum)
        {
            var (toKey, toInt64) = GetEnumDelegates(keyType);
            return (JsonConverter?) Activator.CreateInstance(converterType, toKey, toInt64, options);
        }
        else
        {
            var toInt32Method = GetToInt32Method(keyType);
            if (toInt32Method is null)
                throw new ArgumentException("Invalid type.", nameof(typeToConvert));
            var fromInt32Method = GetFromInt32Method(keyType);
            if (fromInt32Method is null)
                throw new ArgumentException("Invalid type.", nameof(typeToConvert));
            var (toKey, toInt64) = GetInt32ConvertibleDelegates(keyType, toInt32Method, fromInt32Method);
            return (JsonConverter?) Activator.CreateInstance(converterType, toKey, toInt64, options);
        }
    }

    static (Delegate ToKey, Delegate ToInt64) GetEnumDelegates(Type type)
    {
        var enumConverter = typeof(EnumConverter<>).MakeGenericType(type);
        var toEnumField = enumConverter.GetField(nameof(EnumConverter<PlatformID>.ToEnum), BindingFlags.Public | BindingFlags.Static)!;
        var toInt64Field = enumConverter.GetField(nameof(EnumConverter<PlatformID>.ToInt64), BindingFlags.Public | BindingFlags.Static)!;
        return ((Delegate) toEnumField.GetValue(null)!, (Delegate) toInt64Field.GetValue(null)!);
    }

    static (Delegate ToKey, Delegate ToInt64) GetInt32ConvertibleDelegates(Type type, MethodInfo toInt32Method, MethodInfo fromInt32Method)
    {
        var int64Parameter = Parameter(typeof(long));
        var toKeyLambda = Lambda(Call(null, fromInt32Method, Convert(int64Parameter, typeof(int))), int64Parameter);
        var convertibleParameter = Parameter(type);
        var toInt64Lambda = Lambda(Convert(Call(convertibleParameter, toInt32Method), typeof(long)), convertibleParameter);
        return (toKeyLambda.Compile(), toInt64Lambda.Compile());
    }

    static bool IsGenericDictionaryType(Type type)
    {
        if (!type.IsGenericType)
            return false;
        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>) || genericType == typeof(IReadOnlyDictionary<,>);
    }

    static bool IsInt32Convertible(Type type) =>
        GetToInt32Method(type) is not null && GetFromInt32Method(type) is not null;

    static MethodInfo? GetToInt32Method(Type type)
    {
        var query = from method in type.GetMethods()
            where method.Name is "ToInt32" && !method.GetParameters().Any() && method.ReturnType == typeof(int)
            select method;
        return query.SingleOrDefault();
    }

    static MethodInfo? GetFromInt32Method(Type type)
    {
        var query = from method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            where method.Name is "FromInt32" && method.GetParameters().Length is 1 && method.GetParameters()[0].ParameterType == typeof(int) &&
                  method.ReturnType == type
            select method;
        return query.SingleOrDefault();
    }

    class DictionaryEnumConverterInner<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue?>> where TKey : notnull
    {
        readonly Type keyType;
        readonly Func<TKey, long> toInt64;
        readonly Func<long, TKey> toKey;
        readonly JsonConverter<TValue?> valueConverter;
        readonly Type valueType;

        public DictionaryEnumConverterInner(Func<long, TKey> toKey, Func<TKey, long> toInt64, JsonSerializerOptions options)
        {
            this.toKey = toKey;
            this.toInt64 = toInt64;
            valueConverter = (JsonConverter<TValue?>) options.GetConverter(typeof(TValue?));
            keyType = typeof(TKey);
            valueType = typeof(TValue);
        }

        public override Dictionary<TKey, TValue?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var dictionary = new Dictionary<TKey, TValue?>();

            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.EndObject)
                    return dictionary;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                var propertyName = reader.GetString();

                if (!long.TryParse(propertyName, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var key))
                    throw new JsonException($@"Unable to convert ""{propertyName}"" to enum ""{keyType}"".");

                TValue? value;
                if (valueConverter is not null)
                {
                    reader.Read();
                    value = valueConverter.Read(ref reader, valueType, options);
                }
                else
                    value = JsonSerializer.Deserialize<TValue>(ref reader, options);

                dictionary.Add(toKey(key), value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue?> dictionary, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, value) in dictionary)
            {
                var numericKey = toInt64(key);
                writer.WritePropertyName(numericKey.ToString(CultureInfo.InvariantCulture));

                if (valueConverter is not null)
                    valueConverter.Write(writer, value, options);
                else
                    JsonSerializer.Serialize(writer, value, options);
            }

            writer.WriteEndObject();
        }
    }
}
