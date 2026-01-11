using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.GetCreditorsShell;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Bank;

class GetCreditorsEndpoint
{
    GetCreditorsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetCreditorsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from creditors in GetCreditors(entityReader, httpContext.RequestAborted)
            select new GetCreditorsResponse(creditors.Map(CreateCreditor));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static CreditorDto CreateCreditor(User user) =>
        CreateCreditor(user, -user.Accounts[Account.AccountsPayable]);

    static CreditorDto CreateCreditor(User user, Amount accountsPayable) =>
        new(
            CreateUserIdentity(user),
            GetPaymentInformation(user, accountsPayable));

    static PaymentInformation GetPaymentInformation(User user, Amount accountsPayable) =>
        new(
            PaymentIdEncoder.FromUserId(user.UserId),
            accountsPayable,
            GetAccountNumberAllowingNullWhenValueIsMissing(user));

    // There have been cases of mistakes related to pay-outs where the resident
    // had their account number removed but became a creditor after the problem was
    // resolved. It's better to allow the missing account number to be passed to
    // the client instead of returning internal server error on the creditors API.
    static string GetAccountNumberAllowingNullWhenValueIsMissing(User user) => user.AccountNumber.ToNullable()?.ToString()!;
}
