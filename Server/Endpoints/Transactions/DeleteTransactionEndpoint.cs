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
public class DeleteTransactionEndpoint : EndpointBaseAsync.WithRequest<DeleteTransactionServerRequest>.WithActionResult
{
    readonly Func<DeleteTransactionCommand, EitherAsync<Failure, Unit>> deleteTransaction;
    readonly ILogger logger;

    public DeleteTransactionEndpoint(IPersistenceContextFactory contextFactory, ILogger<DeleteTransactionEndpoint> logger)
    {
        this.logger = logger;
        deleteTransaction = command => TransactionsHandler.Handle(command, contextFactory);
    }

    [HttpDelete("transactions/{transactionId:int}")]
    public override Task<ActionResult> HandleAsync([FromRoute] DeleteTransactionServerRequest request, CancellationToken cancellationToken = default)
    {
        var either = deleteTransaction(new(TransactionId.FromInt32(request.TransactionId)));
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
