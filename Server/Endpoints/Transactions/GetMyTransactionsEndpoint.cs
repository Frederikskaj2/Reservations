using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Resident))]
public class GetMyTransactionsEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<MyTransactions>
{
    readonly Func<UserId, EitherAsync<Failure, MyTransactions>> getMyTransactions;
    readonly ILogger logger;

    public GetMyTransactionsEndpoint(
        IPersistenceContextFactory contextFactory, IFormatter formatter, ILogger<GetMyTransactionsEndpoint> logger, IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getMyTransactions = userId => GetMyTransactionsHandler.Handle(formatter, options.Value, contextFactory, userId);
    }

    [HttpGet("transactions")]
    public override Task<ActionResult<MyTransactions>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from transactions in getMyTransactions(userId)
            select transactions;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
