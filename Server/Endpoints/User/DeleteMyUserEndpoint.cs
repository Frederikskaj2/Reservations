using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class DeleteMyUserEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<DeleteUserResponse>
{
    readonly Func<DeleteMyUserCommand, EitherAsync<Failure, DeleteUserResponse>> deleteMyUser;
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;

    public DeleteMyUserEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<DeleteMyUserEndpoint> logger)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        deleteMyUser = command => DeleteMyUserHandler.Handle(contextFactory, emailService, command);
    }

    [HttpDelete("user")]
    public override Task<ActionResult<DeleteUserResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new DeleteMyUserCommand(getTimestamp(), userId)
            from response in deleteMyUser(command)
            select response;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
