using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.BankTransactionFactory;
using static Frederikskaj2.Reservations.Bank.ReconcilePayOutShell;

namespace Frederikskaj2.Reservations.Bank;

class ReconcilePayOutEndpoint
{
    ReconcilePayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int bankTransactionId,
        [FromRoute] int payOutId,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IBankEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<ReconcilePayOutEndpoint> logger,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let command = new ReconcilePayOutCommand(dateProvider.Now, userId, BankTransactionId.FromInt32(bankTransactionId), PayOutId.FromInt32(payOutId))
            from transaction in ReconcilePayOut(emailService, entityReader, entityWriter, command, httpContext.RequestAborted)
            select new ReconcilePayOutResponse(CreateBankTransaction(transaction));
        return either.ToResult(logger, httpContext);
    }
}
