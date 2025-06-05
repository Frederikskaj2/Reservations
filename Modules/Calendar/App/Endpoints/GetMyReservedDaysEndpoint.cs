using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Calendar.GetMyReservedDaysShell;
using static Frederikskaj2.Reservations.Calendar.Validator;

namespace Frederikskaj2.Reservations.Calendar;

class GetMyReservedDaysEndpoint
{
    GetMyReservedDaysEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromQuery] string? fromDate,
        [FromQuery] string? toDate,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetMyReservedDaysEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from query in ValidateGetMyReservedDays(fromDate, toDate, userId).ToAsync()
            from myReservedDays in GetMyReservedDays(entityReader, query, httpContext.RequestAborted)
            select new GetReservedDaysResponse(myReservedDays.Map(CreateReservedDay));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static ReservedDayDto CreateReservedDay(MyReservedDay day) =>
        new(day.Date, day.ResourceId, day.OrderId, day.IsMyReservation);
}
