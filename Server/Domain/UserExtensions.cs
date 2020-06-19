using System;
using System.Linq;
using Frederikskaj2.Reservations.Shared;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal static class UserExtensions
    {
        public static int Balance(this User user, Account account)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return user.AccountBalances.FirstOrDefault(balance => balance.Account == account)?.Amount ?? 0;
        }
    }
}