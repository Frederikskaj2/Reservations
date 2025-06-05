using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.UpdateUserShell;
using static Frederikskaj2.Reservations.Users.UserDetailsFactory;
using static Frederikskaj2.Reservations.Users.Validator;

namespace Frederikskaj2.Reservations.Users;

class UpdateUserEndpoint
{
    UpdateUserEndpoint() { }

    [Authorize(Roles = nameof(Roles.UserAdministration))]
    public static Task<IResult> Handle(
        [FromRoute] int userId,
        [FromBody] UpdateUserRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] ILogger<UpdateUserEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from createdByUserId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateUpdateUser(dateProvider, UserId.FromInt32(userId), request, createdByUserId).ToAsync()
            from result in UpdateUser(jobScheduler, entityReader, entityWriter, command, httpContext.RequestAborted)
            select new UpdateUserResponse(CreateUserDetails(result.User, result.UserFullNames));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
