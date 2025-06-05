using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.GetUserShell;
using static Frederikskaj2.Reservations.Users.UserDetailsFactory;

namespace Frederikskaj2.Reservations.Users;

class GetUserEndpoint
{
    GetUserEndpoint() { }

    [Authorize(Roles = nameof(Roles.UserAdministration))]
    public static Task<IResult> Handle(
        [FromRoute] int userId,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetUserEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from result in GetUser(entityReader, new(UserId.FromInt32(userId)), httpContext.RequestAborted)
            select new GetUserResponse(CreateUserDetails(result.User, result.UserFullNames));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
