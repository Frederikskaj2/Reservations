using Liversage.Primitives;
using System;
using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Orders;

[Primitive]
public readonly partial struct EntryCode
{
    readonly string code;

    public EntryCode(string code)
    {
        if (!IsValid(code))
            throw new ArgumentException("Invalid entry code.", nameof(code));
        this.code = code;
    }

    public static bool IsValid(string code) => EntryCodeRegex.IsMatch(code);

    [GeneratedRegex("^1[13-9][1-9]{4}|[2-9][1-9]{5}$", RegexOptions.None, 1000)]
    static partial Regex EntryCodeRegex { get; }
}
