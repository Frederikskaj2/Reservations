using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.BankTransactionFactory;
using static Frederikskaj2.Reservations.Bank.GetBankTransactionsShell;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class GetBankTransactionsEndpoint
{
    GetBankTransactionsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromQuery] DateOnly? endDate,
        [FromQuery] bool? includeIgnored,
        [FromQuery] bool? includeReconciled,
        [FromQuery] bool? includeUnknown,
        [FromQuery] DateOnly? startDate,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetBankTransactionsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateGetBankTransactions(startDate, endDate, includeUnknown, includeIgnored, includeReconciled).ToAsync()
            from transactions in GetBankTransactions(entityReader, command, httpContext.RequestAborted)
            select new GetBankTransactionsResponse(CreateBankTransactions(transactions));
        return either.ToResult(logger, httpContext);
    }
}
