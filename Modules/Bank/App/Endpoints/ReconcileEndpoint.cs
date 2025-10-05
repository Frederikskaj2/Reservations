using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.BankTransactionFactory;
using static Frederikskaj2.Reservations.Bank.ReconcileShell;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class ReconcileEndpoint
{
    ReconcileEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int bankTransactionId,
        [FromRoute] string? paymentId,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IBankEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] IJobScheduler jobScheduler,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ITimeConverter timeConverter,
        [FromServices] ILogger<ReconcileEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            from command in ValidateReconcile(dateProvider.Now, userId, bankTransactionId, paymentId).ToAsync()
            from transaction in Reconcile(
                emailService, jobScheduler, options.Value, entityReader, timeConverter, entityWriter, command, httpContext.RequestAborted)
            select new ReconcileResponse(CreateBankTransaction(transaction));
        return either.ToResult(logger, httpContext);
    }
}
