using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Calendar.GetOwnerReservedDaysShell;
using static Frederikskaj2.Reservations.Calendar.Validator;

namespace Frederikskaj2.Reservations.Calendar;

class GetOwnerReservedDaysEndpoint
{
    GetOwnerReservedDaysEndpoint() { }

    [Authorize(Roles = nameof(Roles.OrderHandling))]
    public static Task<IResult> Handle(
        [FromQuery] string? fromDate,
        [FromQuery] string? toDate,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ILogger<GetOwnerReservedDaysEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from query in ValidateGetOwnerReservedDays(dateProvider.Today, fromDate, toDate).ToAsync()
            from myReservedDays in GetOwnerReservedDays(options.Value, entityReader, query, httpContext.RequestAborted)
            select new GetReservedDaysResponse(myReservedDays.Map(CreateReservedDay));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static ReservedDayDto CreateReservedDay(MyReservedDay day) =>
        new(day.Date, day.ResourceId, day.OrderId, day.IsMyReservation);
}
