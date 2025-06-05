using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.MyUserResponseFactory;

namespace Frederikskaj2.Reservations.Users;

class GetMyUserEndpoint
{
    GetMyUserEndpoint() { }

    [Authorize]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetMyUserEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New<Unit>(HttpStatusCode.Forbidden, default))
            let query = new GetMyUserQuery(userId)
            from user in GetMyUserShell.GetMyUser(entityReader, query, httpContext.RequestAborted)
            select CreateMyUserResponse(user);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
