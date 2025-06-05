using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Calendar.GetReservedDaysShell;
using static Frederikskaj2.Reservations.Calendar.Validator;

namespace Frederikskaj2.Reservations.Calendar;

class GetReservedDaysEndpoint
{
    GetReservedDaysEndpoint() { }

    public static Task<IResult> Handle(
        [FromQuery] string? fromDate,
        [FromQuery] string? toDate,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetReservedDaysEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from query in ValidateGetReservedDays(fromDate, toDate).ToAsync()
            from myReservedDays in GetReservedDays(entityReader, query, httpContext.RequestAborted)
            select new GetReservedDaysResponse(myReservedDays.Map(CreateMyReservedDay));
        return either.ToResult<Unit, GetReservedDaysResponse>(logger, httpContext, includeFailureDetail: true);
    }

    static ReservedDayDto CreateMyReservedDay(MyReservedDay day) => new(day.Date, day.ResourceId, day.OrderId, day.IsMyReservation);
}
