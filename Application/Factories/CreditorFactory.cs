using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

static class CreditorFactory
{
    public static IEnumerable<Creditor> CreateCreditors(IEnumerable<User> users) =>
        users.Map(CreateCreditor);

    public static Creditor CreateCreditor(User user) =>
        new(
            new(user.UserId, user.Email(), user.FullName, user.Phone, user.ApartmentId),
            user.AccountNumber!,
            -user.Accounts[Account.AccountsPayable]);
}
