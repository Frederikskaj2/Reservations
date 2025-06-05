using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Cleaning.GetCleaningScheduleShell;

namespace Frederikskaj2.Reservations.Cleaning;

class GetCleaningScheduleEndpoint
{
    GetCleaningScheduleEndpoint() { }

    [Authorize(Roles = nameof(Roles.Cleaning))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<GetCleaningScheduleEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from cleaningSchedule in GetCleaningSchedule(entityReader, entityWriter, httpContext.RequestAborted)
            select new GetCleaningScheduleResponse(cleaningSchedule.CleaningTasks.Map(CreateCleaningTask), cleaningSchedule.ReservedDays);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static CleaningTask CreateCleaningTask(CleaningTask cleaningTask) => new(cleaningTask.ResourceId, cleaningTask.Begin, cleaningTask.End);
}
