using Frederikskaj2.Reservations.Shared.Core;
using static Frederikskaj2.Reservations.Application.PaymentIdEncoder;

namespace Frederikskaj2.Reservations.Application;

static class PaymentFunctions
{
    public static PaymentInformation? GetPaymentInformation(OrderingOptions options, User user) =>
        GetPaymentInformation(options, user, user.Accounts[Account.AccountsReceivable]);

    static PaymentInformation? GetPaymentInformation(OrderingOptions options, User user, Amount accountsReceivable) =>
        accountsReceivable > Amount.Zero ? new(FromUserId(user.UserId), accountsReceivable, options.PayInAccountNumber) : null;

}
