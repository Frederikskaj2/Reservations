using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.ImportBankTransactionsShell;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class ImportBankTransactionsEndpoint
{
    ImportBankTransactionsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IBankTransactionsParser bankTransactionsParser,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<ImportBankTransactionsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateFormFile(httpContext, httpContext.RequestAborted)
            from result in ImportBankTransactions(bankTransactionsParser, entityReader, entityWriter, command, httpContext.RequestAborted)
            select new ImportBankTransactionsResponse(result.Count, result.LatestImportStartDate.ToNullable());
        return either.ToResult(logger, httpContext);
    }
}
