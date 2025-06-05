using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Core;

public static class QueryParser
{
    static readonly ILookup<string, string> emptyLookup = Enumerable.Empty<string>().ToLookup(@string => @string, @string => @string);

    public static ILookup<string, string> GetQuery(string uri)
    {
        var index = uri.AsSpan().IndexOf('?');
        if (index is -1)
            return emptyLookup;
        var queryString = uri.AsMemory()[(index + 1)..];
        return SplitQueryString(queryString).ToLookup(
            parameter => Uri.UnescapeDataString(parameter.Name.ToString()),
            parameter => Uri.UnescapeDataString(parameter.Value.ToString()));
    }

    static IEnumerable<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)> SplitQueryString(ReadOnlyMemory<char> queryString)
    {
        if (queryString.Length is 0)
            yield break;
        var start = 0;
        var next = queryString.Span.IndexOf('&');
        while (start < queryString.Length && next >= 0)
        {
            if (next > 0)
                yield return SplitParameter(queryString[start..next]);
            start = start + next + 1;
            next = queryString.Span[start..].IndexOf('&');
        }
        if (start < queryString.Length)
            yield return SplitParameter(queryString[start..]);
    }

    static (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) SplitParameter(ReadOnlyMemory<char> parameter)
    {
        var index = parameter.Span.IndexOf('=');
        return index is -1 ? (parameter, ReadOnlyMemory<char>.Empty) : (parameter[..index], parameter[(index + 1)..]);
    }
}
