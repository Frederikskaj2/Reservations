using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static Frederikskaj2.Reservations.Orders.GetResidentTransactionsShell;
using static Frederikskaj2.Reservations.Orders.TransactionDescriptionFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetUserTransactionsEndpoint
{
    GetUserTransactionsEndpoint() { }

    [Authorize(Roles = nameof(Roles.UserAdministration))]
    public static Task<IResult> Handle(
        [FromRoute] int userId,
        [FromServices] CultureInfo cultureInfo,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetUserTransactionsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from transactions in GetResidentTransactions(entityReader, new(UserId.FromInt32(userId)), httpContext.RequestAborted)
            select CreateResponse(cultureInfo, transactions);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static GetUserTransactionsResponse CreateResponse(CultureInfo cultureInfo, ResidentTransactions residentTransactions) =>
        new(
            residentTransactions.User.UserId,
            residentTransactions.User.FullName,
            FromUserId(residentTransactions.User.UserId),
            residentTransactions.Transactions.Map(transaction => CreateTransaction(cultureInfo, transaction)));

    static TransactionDto CreateTransaction(CultureInfo cultureInfo, Transaction transaction)
    {
        var (orderId, description) = CreateDescription(cultureInfo, transaction);
        return new(
            transaction.TransactionId,
            transaction.Date,
            transaction.Activity,
            orderId,
            description,
            -(transaction.Amounts[Account.AccountsReceivable] + transaction.Amounts[Account.AccountsPayable]));
    }
}
