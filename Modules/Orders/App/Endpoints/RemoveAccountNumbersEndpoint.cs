using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.RemoveAccountNumbersShell;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

class RemoveAccountNumbersEndpoint
{
    RemoveAccountNumbersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<RemoveAccountNumbersEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from _ in RemoveAccountNumbers(options.Value, entityReader, entityWriter, new(dateProvider.Now, userId), httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
