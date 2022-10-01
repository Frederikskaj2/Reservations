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

[Authorize(Roles = nameof(Roles.UserAdministration))]
public class UpdateUserEndpoint : EndpointBaseAsync.WithRequest<UpdateUserServerRequest>.WithActionResult<UpdateUserResponse>
{
    readonly ILogger logger;
    readonly Func<UpdateUserCommand, EitherAsync<Failure, UpdateUserResponse>> updateUser;
    readonly Func<UserId, UpdateUserRequest?, UserId, EitherAsync<Failure, UpdateUserCommand>> validateRequest;

    public UpdateUserEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<UpdateUserEndpoint> logger)
    {
        this.logger = logger;
        validateRequest = (userId, request, administratorUserId) =>
            Validator.ValidateUpdateUser(dateProvider, userId, request, administratorUserId).ToAsync();
        updateUser = command => UpdateUserHandler.Handle(contextFactory, emailService, command);
    }

    [HttpPatch("users/{userId:int}")]
    public override Task<ActionResult<UpdateUserResponse>> HandleAsync(
        [FromRoute] UpdateUserServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from administratorUserId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(UserId.FromInt32(request.UserId), request.Body, administratorUserId)
            from order in updateUser(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
