using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Persistence;

static partial class OneOfFunctions
{
    [GeneratedRegex("^AsT(?<index>[0-8])$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AsTNumberRegex();

    [GeneratedRegex("^IsT(?<index>[0-8])$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IsTNumberRegex();

    [GeneratedRegex("^OneOf.OneOfBase`[1-9]$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OneOfBaseGenericTypeNameRegex();

    [GeneratedRegex("^OneOf.OneOf`[1-9]$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OneOfGenericTypeNameRegex();

    public static bool IsOneOfType(Type? type)
    {
        if (type is null)
            return false;
        if (!type.IsGenericType)
            return FindOneOfBaseType(type) is not null;
        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition.FullName is not null && OneOfGenericTypeNameRegex().IsMatch(genericTypeDefinition.FullName);
    }

    public static bool IsIsTNumberMember(string memberName, out int index)
    {
        var match = IsTNumberRegex().Match(memberName);
        if (match.Success)
        {
            index = int.Parse(match.Groups["index"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
            return true;
        }
        index = 0;
        return false;
    }

    public static bool IsAsTNumberMember(string memberName, out int index)
    {
        var match = AsTNumberRegex().Match(memberName);
        if (match.Success)
        {
            index = int.Parse(match.Groups["index"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
            return true;
        }
        index = 0;
        return false;
    }

    public static string GetOneOfSerializationPropertyName(Type oneOfType, int oneOfIndex)
    {
        var genericType = oneOfType.IsGenericType ? oneOfType : FindOneOfBaseType(oneOfType)!;
        var genericArgument = genericType.GetGenericArguments()[oneOfIndex];
        return genericArgument.Name;
    }

    static Type? FindOneOfBaseType(Type? type)
    {
        while (type is not null)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var match = OneOfBaseGenericTypeNameRegex().Match(genericTypeDefinition.FullName ?? "");
                if (match.Success)
                    return type;
            }
            type = type.BaseType;
        }
        return null;
    }
}
