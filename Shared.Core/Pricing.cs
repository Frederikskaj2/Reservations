using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class Pricing
{
    public static Price GetPrice(OrderingOptions options, IReadOnlySet<LocalDate> holidays, Extent extent, ResourceType resourceType)
    {
        var priceOptions = options.Prices[resourceType];
        var numberOfHighPriceNights = GetNumberOfHighPriceNights(holidays, extent);
        var rent = numberOfHighPriceNights*priceOptions.HighRentPerNight + (extent.Nights - numberOfHighPriceNights)*priceOptions.LowRentPerNight;
        var deposit = numberOfHighPriceNights > 0 ? priceOptions.HighDeposit : priceOptions.LowDeposit;
        return new Price(rent, priceOptions.Cleaning, deposit);
    }

    static int GetNumberOfHighPriceNights(IReadOnlySet<LocalDate> holidays, Extent extent) =>
        Enumerable.Range(0, extent.Nights).Aggregate(
            0,
            (count, i) => count + (HighPricePolicy.IsHighPriceDay(extent.Date.PlusDays(i), holidays) ? 1 : 0));
}
