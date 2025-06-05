using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.BankTransactionFactory;
using static Frederikskaj2.Reservations.Bank.UpdateBankTransactionShell;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class UpdateBankTransactionEndpoint
{
    UpdateBankTransactionEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int bankTransactionId,
        [FromBody] UpdateBankTransactionRequest request,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<UpdateBankTransactionEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateUpdateBankTransaction(bankTransactionId, request).ToAsync()
            from transaction in UpdateBankTransaction(entityReader, entityWriter, command, httpContext.RequestAborted)
            select new UpdateBankTransactionResponse(CreateBankTransaction(transaction));
        return either.ToResult(logger, httpContext);
    }
}
