using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Cleaning.SendCleaningScheduleShell;

namespace Frederikskaj2.Reservations.Cleaning;

class SendCleaningScheduleEndpoint
{
    SendCleaningScheduleEndpoint() { }

    [Authorize(Roles = nameof(Roles.Cleaning))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] ICleaningEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<SendCleaningScheduleEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from email in SendCleaningSchedule(emailService, entityReader,  new(userId, dateProvider.Today), httpContext.RequestAborted)
            select new SendCleaningScheduleResponse(email);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
