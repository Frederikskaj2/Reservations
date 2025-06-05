using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

class DeleteMyUserEndpoint
{
    DeleteMyUserEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<DeleteMyUserEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let command = new DeleteMyUserCommand(dateProvider.Now, userId)
            from result in DeleteMyUserShell.DeleteMyUser(jobScheduler, entityReader, entityWriter, command, httpContext.RequestAborted)
            select new DeleteUserResponse(result);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
