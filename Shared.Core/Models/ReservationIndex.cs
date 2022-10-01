using Liversage.Primitives;
using System;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Equatable | Features.Comparable)]
public readonly partial struct ReservationIndex
{
    readonly int index;

    public static bool TryParse(string @string, out ReservationIndex reservationIndex) =>
        TryParse(@string.AsSpan(), out reservationIndex);

    public static bool TryParse(ReadOnlySpan<char> span, out ReservationIndex reservationIndex)
    {
        if (int.TryParse(span, out var id))
        {
            reservationIndex = id;
            return true;
        }
        reservationIndex = default;
        return false;
    }}
