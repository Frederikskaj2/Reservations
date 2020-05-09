using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class MyTransactionService
    {
        private readonly ReservationsContext db;

        public MyTransactionService(ReservationsContext db) => this.db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<IEnumerable<MyTransaction>> GetMyTransactions(int userId)
        {
            var transactions = await db.Transactions
                .Include(transaction => transaction.Amounts)
                .Where(transaction => transaction.UserId == userId)
                .OrderBy(transaction => transaction.Id)
                .ToListAsync();
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return transactions
                .Select(CreateMyTransaction)
                .Where(myTransaction => myTransaction != null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

            static MyTransaction? CreateMyTransaction(Transaction transaction)
            {
                if (transaction.Amount(Account.FromPayments) != 0 || transaction.Amount(Account.ToAccountsReceivable) != 0)
                    return null;

                var bank = transaction.Amount(Account.Bank);
                if (bank > 0)
                    return new MyTransaction
                    {
                        Date = transaction.Date,
                        Type = TransactionType.PayIn,
                        OrderId = transaction.OrderId,
                        Amount = bank
                    };
                else if (bank < 0)
                    return new MyTransaction
                    {
                        Date = transaction.Date,
                        Type = TransactionType.PayOut,
                        OrderId = transaction.OrderId,
                        Amount = bank
                    };

                var deposits = transaction.Amount(Account.Deposits);
                if (deposits >= 0)
                {
                    var rent = transaction.Amount(Account.Rent);
                    var cleaning = transaction.Amount(Account.Cleaning);
                    var cancellationFees = transaction.Amount(Account.CancellationFees);
                    var damages = transaction.Amount(Account.Damages);
                    if (rent + cleaning + cancellationFees == 0)
                        return new MyTransaction
                        {
                            Date = transaction.Date,
                            Type = TransactionType.Settlement,
                            OrderId = transaction.OrderId,
                            ResourceId = transaction.ResourceId,
                            ReservationDate = transaction.ReservationDate,
                            Amount = deposits + damages
                        };

                    return new MyTransaction
                    {
                        Date = transaction.Date,
                        Type = TransactionType.Cancellation,
                        OrderId = transaction.OrderId,
                        ResourceId = transaction.ResourceId,
                        ReservationDate = transaction.ReservationDate,
                        Amount = deposits + rent + cleaning + cancellationFees
                    };
                }
                else
                {
                    var rent = transaction.Amount(Account.Rent);
                    var cleaning = transaction.Amount(Account.Cleaning);
                    return new MyTransaction
                    {
                        Date = transaction.Date,
                        Type = TransactionType.Order,
                        OrderId = transaction.OrderId,
                        Amount = rent + cleaning + deposits
                    };
                }
            }
        }
    }
}
