using Frederikskaj2.Reservations.LockBox;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Orders;

public static class Pricing
{
    public static Price GetPrice(OrderingOptions options, IReadOnlySet<LocalDate> holidays, Extent extent, ResourceType resourceType)
    {
        var priceOptions = options.Prices[resourceType];
        var numberOfHighPriceNights = GetNumberOfHighPriceNights(holidays, extent);
        var rent = numberOfHighPriceNights*priceOptions.HighRentPerNight + (extent.Nights - numberOfHighPriceNights)*priceOptions.LowRentPerNight;
        var cleaning = priceOptions.Cleaning + (HighPricePolicy.IsSurchargedCleaningDay(extent.Ends(), holidays) ? priceOptions.CleaningSurcharge : 0);
        var deposit = numberOfHighPriceNights > 0 ? priceOptions.HighDeposit : priceOptions.LowDeposit;
        return new(rent, cleaning, deposit);
    }

    static int GetNumberOfHighPriceNights(IReadOnlySet<LocalDate> holidays, Extent extent) =>
        Enumerable.Range(0, extent.Nights).Aggregate(
            0,
            (count, i) => count + (HighPricePolicy.IsHighPriceDay(extent.Date.PlusDays(i), holidays) ? 1 : 0));
}
