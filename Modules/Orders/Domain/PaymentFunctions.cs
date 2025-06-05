using Frederikskaj2.Reservations.Users;
using LanguageExt;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class PaymentFunctions
{
    public static Option<PaymentInformation> GetPaymentInformation(OrderingOptions options, User user) =>
        GetPaymentInformation(options, user, user.Balance());

    static Option<PaymentInformation> GetPaymentInformation(OrderingOptions options, User user, Amount balance) =>
        balance > Amount.Zero ? new PaymentInformation(FromUserId(user.UserId), balance, options.PayInAccountNumber) : None;
}
