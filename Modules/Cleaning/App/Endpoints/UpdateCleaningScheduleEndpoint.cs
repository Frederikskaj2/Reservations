using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Cleaning.UpdateCleaningScheduleShell;

namespace Frederikskaj2.Reservations.Cleaning;

class UpdateCleaningScheduleEndpoint
{
    UpdateCleaningScheduleEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ILogger<UpdateCleaningScheduleEndpoint> logger,
        HttpContext httpContext)
    {
        var either = UpdateCleaningSchedule(options.Value, entityReader, entityWriter, new(dateProvider.Today), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
