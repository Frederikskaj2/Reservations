using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;
using System;
using System.Globalization;

namespace Frederikskaj2.Reservations.Users;

[Primitive(Features.Equatable | Features.Comparable | Features.Formattable)]
public readonly partial struct OrderId : IIsId
{
    readonly int id;

    public string GetId() => ToString();

    public static bool TryParse(string @string, out OrderId orderId) =>
        TryParse(@string.AsSpan(), out orderId);

    public static bool TryParse(ReadOnlySpan<char> span, out OrderId orderId)
    {
        if (int.TryParse(span, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            orderId = id;
            return true;
        }
        orderId = default;
        return false;
    }
}
