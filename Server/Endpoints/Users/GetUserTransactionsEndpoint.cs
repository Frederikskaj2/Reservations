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
public class GetUserTransactionsEndpoint : EndpointBaseAsync.WithRequest<UserServerRequest>.WithActionResult<UserTransactions>
{
    readonly Func<UserId, EitherAsync<Failure, UserTransactions>> getMyTransactions;
    readonly ILogger logger;

    public GetUserTransactionsEndpoint(IPersistenceContextFactory contextFactory, IFormatter formatter, ILogger<GetUserTransactionsEndpoint> logger)
    {
        this.logger = logger;
        getMyTransactions = userId => GetUserTransactionsHandler.Handle(contextFactory, formatter, userId);
    }

    [HttpGet("users/{userId:int}/transactions")]
    public override Task<ActionResult<UserTransactions>> HandleAsync([FromRoute] UserServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = getMyTransactions(UserId.FromInt32(request.UserId));
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
