using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client
{
    internal static class QueryParser
    {
        private static readonly ILookup<string, string> emptyLookup = Enumerable.Empty<string>().ToLookup(@string => @string, @string => @string);

        public static ILookup<string, string> GetQuery(string uri)
        {
            var index = uri.AsSpan().IndexOf('?');
            if (index == -1)
                return emptyLookup;
            var queryString = uri.AsMemory().Slice(index + 1);
            return SplitQueryString(queryString).ToLookup(parameter => Uri.UnescapeDataString(parameter.Name.ToString()), parameter => Uri.UnescapeDataString(parameter.Value.ToString()));
        }

        private static IEnumerable<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)> SplitQueryString(ReadOnlyMemory<char> queryString)
        {
            if (queryString.Length == 0)
                yield break;
            var start = 0;
            var next = queryString.Span.IndexOf('&');
            while (start < queryString.Length && next >= 0)
            {
                if (next > 0)
                    yield return SplitParameter(queryString.Slice(start, next));
                start = start + next + 1;
                next = queryString.Span.Slice(start).IndexOf('&');
            }
            if (start < queryString.Length)
                yield return SplitParameter(queryString.Slice(start));
        }

        private static (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) SplitParameter(ReadOnlyMemory<char> parameter)
        {
            var index = parameter.Span.IndexOf('=');
            if (index == -1)
                return (parameter, ReadOnlyMemory<char>.Empty);
            else
                return (parameter.Slice(0, index), parameter.Slice(index + 1));
        }
    }
}
