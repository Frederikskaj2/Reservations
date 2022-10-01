using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
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
public class GetMyUserEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<MyUser>
{
    readonly Func<UserId, EitherAsync<Failure, MyUser>> getUser;
    readonly ILogger logger;

    public GetMyUserEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetMyUserEndpoint> logger)
    {
        this.logger = logger;
        getUser = userId => GetMyUserHandler.Handle(contextFactory, userId);
    }

    [HttpGet("user")]
    public override Task<ActionResult<MyUser>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from user in getUser(userId)
            select user;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
