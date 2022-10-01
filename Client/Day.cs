using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client;

public record Day(
    LocalDate Date,
    bool IsToday,
    bool IsCurrentMonth,
    bool IsHighPriceDay,
    bool IsResourceAvailable,
    IReadOnlyDictionary<ResourceId, (bool IsMyReservation, OrderId? OrderId)> ReservedResources)
{
    public static readonly IReadOnlyDictionary<ResourceId, (bool IsMyReservation, OrderId? OrderId)> EmptyReservedResources =
        new Dictionary<ResourceId, (bool IsMyReservation, OrderId? OrderId)>();
}
