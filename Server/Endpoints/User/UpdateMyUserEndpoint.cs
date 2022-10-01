using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class UpdateMyUserEndpoint : EndpointBaseAsync.WithRequest<UpdateMyUserRequest>.WithActionResult<MyUser>
{
    readonly ILogger logger;
    readonly Func<UpdateMyUserCommand, EitherAsync<Failure, MyUser>> updateMyUser;
    readonly Func<UpdateMyUserRequest, UserId, EitherAsync<Failure, UpdateMyUserCommand>> validateRequest;

    public UpdateMyUserEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<UpdateMyUserEndpoint> logger)
    {
        this.logger = logger;
        validateRequest = (request, userId) => Validator.ValidateUpdateMyUser(dateProvider, request, userId).ToAsync();
        updateMyUser = command => UpdateMyUserHandler.Handle(contextFactory, command);
    }

    [HttpPatch("user")]
    public override Task<ActionResult<MyUser>> HandleAsync(UpdateMyUserRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(request, userId)
            from user in updateMyUser(command)
            select user;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
