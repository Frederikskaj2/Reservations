using Liversage.Primitives;
using System;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Equatable | Features.Comparable | Features.Formattable)]
public readonly partial struct OrderId
{
    readonly int id;

    public static bool TryParse(string @string, out OrderId orderId) =>
        TryParse(@string.AsSpan(), out orderId);

    public static bool TryParse(ReadOnlySpan<char> span, out OrderId orderId)
    {
        if (int.TryParse(span, out var id))
        {
            orderId = id;
            return true;
        }
        orderId = default;
        return false;
    }
}
