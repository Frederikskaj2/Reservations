using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Diagnostics;
using System.Linq;

namespace Frederikskaj2.Reservations.Orders;

static class GetYearlySummary
{
    public static GetYearlySummaryOutput GetYearlySummaryCore(GetYearlySummaryInput input) =>
        new(input.Query.Year, GetResourceSummaries(input.Reservations));

    static Seq<ResourceSummary> GetResourceSummaries(Seq<Reservation> reservations) =>
        reservations
            .GroupBy(GetResourceType)
            .Select(GetResourceSummary)
            .ToSeq();

    static ResourceType GetResourceType(Reservation reservation) =>
        Resources.GetResourceType(reservation.ResourceId).Case switch
        {
            ResourceType type => type,
            _ => throw new UnreachableException(),
        };

    static ResourceSummary GetResourceSummary(IGrouping<ResourceType, Reservation> grouping)
    {
        var (reservationCount, nights, income) = grouping.Aggregate(
            (ReservationCount: 0, Nights: 0, Income: Amount.Zero),
            (sum, reservation) => (sum.ReservationCount + 1, sum.Nights + reservation.Extent.Nights, sum.Income + GetIncome(reservation)));
        return new(grouping.Key, reservationCount, nights, income);
    }

    static Amount GetIncome(Reservation reservation) =>
        reservation.Price.Case switch
        {
            Price price => price.Rent + price.Cleaning,
            _ => throw new UnreachableException(),
        };
}
