using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewUsers)]
public class GetUserEndpoint : EndpointBaseAsync.WithRequest<UserServerRequest>.WithActionResult<UserDetails>
{
    readonly Func<UserId, EitherAsync<Failure, UserDetails>> getUser;
    readonly ILogger logger;

    public GetUserEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetUserEndpoint> logger)
    {
        this.logger = logger;
        getUser = userId => GetUserHandler.Handle(contextFactory, userId);
    }

    [HttpGet("users/{userId:int}")]
    public override Task<ActionResult<UserDetails>> HandleAsync([FromRoute] UserServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = getUser(UserId.FromInt32(request.UserId));
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
