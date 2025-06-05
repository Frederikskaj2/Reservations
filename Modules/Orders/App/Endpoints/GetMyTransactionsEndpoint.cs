using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetMyTransactionsShell;
using static Frederikskaj2.Reservations.Orders.TransactionDescriptionFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetMyTransactionsEndpoint
{
    GetMyTransactionsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Resident))]
    public static Task<IResult> Handle(
        [FromServices] CultureInfo cultureInfo,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetMyTransactionsEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        ClaimsPrincipal claimsPrincipal,
        HttpContext httpContext)
    {
        var either =
            from userId in claimsPrincipal.UserId().ToEitherAsync(Failure.New(HttpStatusCode.Forbidden))
            let query = new GetMyTransactionsQuery(userId)
            from transactions in GetMyTransactions(options.Value, entityReader, query, httpContext.RequestAborted)
            select CreateResponse(cultureInfo, transactions);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static GetMyTransactionsResponse CreateResponse(CultureInfo cultureInfo, MyTransactions myTransactions) =>
        new(
            myTransactions.Transactions.Map(transaction => CreateMyTransaction(cultureInfo, transaction)),
            myTransactions.Payment.ToNullableReference());

    static MyTransactionDto CreateMyTransaction(CultureInfo cultureInfo, Transaction transaction)
    {
        var (orderId, description) = CreateDescription(cultureInfo, transaction);
        return new(
            transaction.Date,
            transaction.Activity,
            orderId,
            description,
            -(transaction.Amounts[Account.AccountsReceivable] + transaction.Amounts[Account.AccountsPayable]));
    }
}
