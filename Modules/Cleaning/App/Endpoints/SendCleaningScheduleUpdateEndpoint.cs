using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Cleaning.SendCleaningScheduleUpdateShell;

namespace Frederikskaj2.Reservations.Cleaning;

class SendCleaningScheduleUpdateEndpoint
{
    SendCleaningScheduleUpdateEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] ICleaningEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SendCleaningScheduleUpdateEndpoint> logger,
        HttpContext httpContext)
    {
        var either = SendCleaningScheduleUpdate(emailService, entityReader, entityWriter, new(dateProvider.Today), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
