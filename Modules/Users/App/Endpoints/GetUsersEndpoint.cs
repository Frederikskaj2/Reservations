using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.GetUsersShell;

namespace Frederikskaj2.Reservations.Users;

class GetUsersEndpoint
{
    GetUsersEndpoint() { }

    [Authorize(Policy = Policy.ViewUsers)]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetUsersEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from users in GetUsers(entityReader, httpContext.RequestAborted)
            select users.Map(UserFactory.CreateUser);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
