using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.EmailSender;

static class TypeExtensions
{
    static readonly Dictionary<Type, string> aliases = new()
    {
        { typeof(object), "object" },
        { typeof(string), "string" },
        { typeof(char), "char" },
        { typeof(bool), "bool" },
        { typeof(sbyte), "sbyte" },
        { typeof(byte), "byte" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(void), "void" }
    };

    static readonly ConcurrentDictionary<Type, string> csharpNameCache = new();

    public static string CSharpName(this Type type, bool includeNamespaces = false)
    {
        return csharpNameCache.GetOrAdd(type, _ => GetName());

        string GetName()
        {
            if (aliases.TryGetValue(type, out var alias))
                return alias;

            if (type.IsArray)
                return type.GetElementType()!.CSharpName(includeNamespaces) + "[]";

            if (!type.IsGenericType)
                return GetTypeName(type.Namespace, type.Name, includeNamespaces);

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0].CSharpName(includeNamespaces) + "?";

            var nonGenericName = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)];
            var genericArgumentNames = string.Join(", ", type.GetGenericArguments().Select(t => t.CSharpName(includeNamespaces)));
            return $"{GetTypeName(type.Namespace, nonGenericName, includeNamespaces)}<{genericArgumentNames}>";
        }

        static string GetTypeName(string? namespaceName, string typeName, bool includeNamespaces) =>
            includeNamespaces && !string.IsNullOrEmpty(namespaceName) ? $"{namespaceName}.{typeName}" : typeName;
    }
}