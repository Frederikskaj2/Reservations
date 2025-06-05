using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

static class GetYearlySummaryRange
{
    public static GetYearlySummaryRangeOutput GetYearlySummaryRangeCore(GetYearlySummaryRangeInput input) =>
        new(GetRange(input));

    static YearlySummaryRange GetRange(GetYearlySummaryRangeInput input) =>
        input.EarliestReservationDate.Case switch
        {
            LocalDate date => new(date.Year, input.Query.Today.Year),
            _ => new(input.Query.Today.Year, input.Query.Today.Year),
        };
}
