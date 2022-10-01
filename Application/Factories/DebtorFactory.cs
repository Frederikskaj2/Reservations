using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.PaymentIdEncoder;

namespace Frederikskaj2.Reservations.Application;

static class DebtorFactory
{
    public static IEnumerable<Debtor> CreateDebtors(IEnumerable<User> users) =>
        users.Map(CreateDebtor).OrderBy(debtor => debtor.PaymentId);

    public static Debtor CreateDebtor(User user) =>
        CreateDebtor(user, user.Accounts[Account.AccountsReceivable]);

    static Debtor CreateDebtor(User user, Amount accountsReceivable) =>
        new(
            FromUserId(user.UserId),
            new(
                user.UserId,
                user.Email(),
                user.FullName,
                user.Phone,
                user.ApartmentId),
            accountsReceivable > Amount.Zero ? accountsReceivable : Amount.Zero);

}
