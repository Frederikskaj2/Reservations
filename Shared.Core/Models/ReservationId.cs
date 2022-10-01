using System;

namespace Frederikskaj2.Reservations.Shared.Core;

public readonly struct ReservationId : IEquatable<ReservationId>
{
    public ReservationId(OrderId orderId, ReservationIndex index)
    {
        OrderId = orderId;
        Index = index;
    }

    public OrderId OrderId { get; }
    public ReservationIndex Index { get; }

    public bool Equals(ReservationId other) => OrderId == other.OrderId && Index == other.Index;

    public override bool Equals(object? obj) => obj is ReservationId other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(OrderId, Index);

    public override string ToString() => $"{OrderId}[{Index}]";

    public static bool operator ==(ReservationId reservationId1, ReservationId reservationId2) =>
        reservationId1.Equals(reservationId2);

    public static bool operator !=(ReservationId reservationId1, ReservationId reservationId2) =>
        !(reservationId1 == reservationId2);

    public static bool TryParse(string @string, out ReservationId reservationId) =>
        TryParse(@string.AsSpan(), out reservationId);

    public static bool TryParse(ReadOnlySpan<char> span, out ReservationId reservationId)
    {
        var leftBracketIndex = span.IndexOf('[');
        if (leftBracketIndex < 0)
        {
            reservationId = default;
            return false;
        }
        var rightBracketIndex = span.IndexOf(']');
        if (rightBracketIndex != span.Length - 1)
        {
            reservationId = default;
            return false;
        }
        var orderIdSpan = span[..leftBracketIndex];
        if (!OrderId.TryParse(orderIdSpan, out var orderId))
        {
            reservationId = default;
            return false;
        }
        var indexSpan = span[(leftBracketIndex + 1)..rightBracketIndex];
        if (!ReservationIndex.TryParse(indexSpan, out var index))
        {
            reservationId = default;
            return false;
        }
        reservationId = new ReservationId(orderId, index);
        return true;
    }
}
