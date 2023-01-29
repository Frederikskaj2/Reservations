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

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class GetCreditorEndpoint : EndpointBaseAsync.WithRequest<UserServerRequest>.WithActionResult<Creditor>
{
    readonly Func<UserId, EitherAsync<Failure, Creditor>> getCreditor;
    readonly ILogger logger;

    public GetCreditorEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetCreditorEndpoint> logger)
    {
        this.logger = logger;
        getCreditor = userId => GetCreditorHandler.Handle(contextFactory, userId);
    }

    [HttpGet("creditors/{userId:int}")]
    public override Task<ActionResult<Creditor>> HandleAsync([FromRoute] UserServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = getCreditor(UserId.FromInt32(request.UserId));
        return either.ToResultAsync(logger, HttpContext);

    }
}
