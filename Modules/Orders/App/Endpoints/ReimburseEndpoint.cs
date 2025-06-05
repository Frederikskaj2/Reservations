using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.ReimburseShell;
using static Frederikskaj2.Reservations.Orders.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

class ReimburseEndpoint
{
    ReimburseEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int userId,
        [FromBody] ReimburseRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<ReimburseEndpoint> logger,
        [FromServices] ITimeConverter timeConverter,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var timestamp = dateProvider.Now;
        var either =
            from administratorUserId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateReimburse(timeConverter, timestamp, UserId.FromInt32(userId), request, administratorUserId).ToAsync()
            from _ in Reimburse(jobScheduler, entityReader, entityWriter, command, httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
