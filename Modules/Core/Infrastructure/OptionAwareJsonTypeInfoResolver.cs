using LanguageExt;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization.Metadata;

namespace Frederikskaj2.Reservations.Core;

class OptionAwareJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    readonly ConcurrentDictionary<Type, Func<object, object?, bool>> predicates = new();

    public OptionAwareJsonTypeInfoResolver() =>
        Modifiers.Add(
            info =>
            {
                if (info.Type is not { IsValueType: false, IsArray: false })
                    return;
                var optionProperties = info.Properties
                    .Where(property => property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Option<>));
                foreach (var property in optionProperties)
                    property.ShouldSerialize = GetShouldSerializePredicate(property.PropertyType);
            });

    Func<object, object?, bool> GetShouldSerializePredicate(Type optionType) =>
        predicates.GetOrAdd(optionType, CreateShouldSerializePredicate);

    static Func<object, object?, bool> CreateShouldSerializePredicate(Type optionType)
    {
        var parameter1 = Expression.Parameter(typeof(object));
        var parameter2 = Expression.Parameter(typeof(object));
        var convert = Expression.Convert(parameter2, optionType);
        var property = Expression.Property(convert, optionType.GetProperty("IsSome")!);
        var lambda = Expression.Lambda(property, parameter1, parameter2);
        return (Func<object, object?, bool>) lambda.Compile();
    }
}
