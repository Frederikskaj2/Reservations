using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.MyUserResponseFactory;
using static Frederikskaj2.Reservations.Users.UpdateMyUserShell;
using static Frederikskaj2.Reservations.Users.Validator;

namespace Frederikskaj2.Reservations.Users;

class UpdateMyUserEndpoint
{
    UpdateMyUserEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromBody] UpdateMyUserRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<UpdateMyUserEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateUpdateMyUser(dateProvider, request, userId).ToAsync()
            from user in UpdateMyUser(entityReader, entityWriter, command, httpContext.RequestAborted)
            select CreateMyUserResponse(user);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
