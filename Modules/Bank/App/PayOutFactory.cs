using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static Frederikskaj2.Reservations.Users.UserIdentityFactory;

namespace Frederikskaj2.Reservations.Bank;

static class PayOutFactory
{
    public static IEnumerable<PayOutDto> CreatePayOuts(Seq<PayOutDetails> details) =>
        details.Map(CreatePayOut);

    public static PayOutDto CreatePayOut(PayOutDetails details) =>
        new(
            details.PayOut.PayOutId,
            details.PayOut.Timestamp,
            CreateUserIdentity(details.User),
            FromUserId(details.User.UserId),
            details.PayOut.Amount,
            details.ETag);
}
