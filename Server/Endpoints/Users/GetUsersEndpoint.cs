using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Policy = Policies.ViewUsers)]
public class GetUsersEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<User>>
{
    readonly Func<EitherAsync<Failure, IEnumerable<User>>> getUsers;
    readonly ILogger logger;

    public GetUsersEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetUsersEndpoint> logger)
    {
        this.logger = logger;
        getUsers = () => GetUsersHandler.Handle(contextFactory);
    }

    [HttpGet("users")]
    public override Task<ActionResult<IEnumerable<User>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getUsers();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
