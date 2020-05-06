using System;
using System.Linq;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal static class TransactionExtensions
    {
        public static int Amount(this Transaction transaction, Account account)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            return transaction.Amounts.FirstOrDefault(amount => amount.Account == account)?.Amount ?? 0;
        }
    }
}