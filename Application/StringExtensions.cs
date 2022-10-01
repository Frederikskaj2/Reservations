using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Application;

static class StringExtensions
{
    [return: NotNullIfNotNull("string")]
    public static string? ToCamelCase(this string? @string)
    {
        if (@string is null)
            return null;

        // "abcDef" => "abcDef"
        // "AbcDef" => "abcDef"
        // "ABCDef" => "abcDef"
        // "ABCDEF" => "abcdef"
        if (@string.Length == 0 || char.IsLower(@string[0]))
            return @string;

        var indexOfFirstLowerAfterUpper = -1;
        for (var i = 1; i < @string.Length; i += 1)
            if (char.IsUpper(@string[i - 1]) && char.IsLower(@string[i]))
            {
                indexOfFirstLowerAfterUpper = i;
                break;
            }

        return indexOfFirstLowerAfterUpper switch
        {
            1 => string.Create(@string.Length, (@string, indexOfFirstLowerAfterUpper), StartToLowerSpanAction),
            >= 2 => string.Create(@string.Length, (@string, indexOfFirstLowerAfterUpper - 1), StartToLowerSpanAction),
            _ => string.Create(@string.Length, @string, AllToLowerSpanAction)
        };

        static void StartToLowerSpanAction(Span<char> span, (string Source, int Index) tuple)
        {
            var (source, index) = tuple;
            source.AsSpan(0, index).ToLowerInvariant(span);
            source.AsSpan(index).CopyTo(span[index..]);
        }

        static void AllToLowerSpanAction(Span<char> span, string source) => source.AsSpan().ToLowerInvariant(span);
    }
}