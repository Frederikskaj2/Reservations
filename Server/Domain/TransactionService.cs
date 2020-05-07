using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using NodaTime;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using Posting = Frederikskaj2.Reservations.Server.Data.Posting;
using Price = Frederikskaj2.Reservations.Server.Data.Price;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    [SuppressMessage(
        "Performance", "CA1822:Mark members as static",
        Justification = "This service is injected into consumers and members should be accessed via this reference.")]
    public class TransactionService
    {
        public void CreateOrderTransaction(Instant timestamp, LocalDate date, Order order, Price price)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));
            if (price is null)
                throw new ArgumentNullException(nameof(price));

            var amounts = new List<TransactionAmount>()
            {
                new TransactionAmount { Account = Account.Rent, Amount = -price.Rent },
                new TransactionAmount { Account = Account.Cleaning, Amount = -price.Cleaning },
                new TransactionAmount { Account = Account.Deposits, Amount = -price.Deposit },
                new TransactionAmount { Account = Account.AccountsReceivable, Amount = price.GetTotal() }
            };
            CreateTransaction(timestamp, date, order.Id, order, amounts);
        }

        public void CreateReservationCancelledTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order order, Reservation reservation, int cancellationFee)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));
            if (cancellationFee < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            var accountsReceivable = order.User!.Balance(Account.AccountsReceivable);
            var amountToRefund = reservation.Price!.GetTotal() - cancellationFee;

            var amounts = new List<TransactionAmount>()
                {
                    new TransactionAmount { Account = Account.Rent, Amount = reservation.Price.Rent },
                    new TransactionAmount { Account = Account.Cleaning, Amount = reservation.Price.Cleaning },
                    new TransactionAmount { Account = Account.Deposits, Amount = reservation.Price.Deposit },
                };
            if (cancellationFee > 0)
                amounts.Add(new TransactionAmount { Account = Account.CancellationFees, Amount = -cancellationFee });
            if (accountsReceivable > 0)
            {
                var amount = Math.Min(accountsReceivable, amountToRefund);
                amounts.Add(new TransactionAmount { Account = Account.AccountsReceivable, Amount = -amount });
                if (amountToRefund > accountsReceivable)
                    amounts.Add(new TransactionAmount { Account = Account.Payments, Amount = -(amountToRefund - accountsReceivable) });
            }
            else
            {
                amounts.Add(new TransactionAmount { Account = Account.Payments, Amount = -amountToRefund });
            }

            CreateTransaction(timestamp, date, createdByUserId, order, amounts);
        }

        public void CreatePayInTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order order, int amount)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var accountsReceivable = Math.Min(amount, order.User!.Balance(Account.AccountsReceivable));
            var amounts = new List<TransactionAmount>()
            {
                new TransactionAmount { Account = Account.Bank, Amount = amount },
            };
            if (amount <= accountsReceivable)
            {
                amounts.Add(new TransactionAmount { Account = Account.AccountsReceivable, Amount = -amount });
            }
            else
            {
                amounts.Add(new TransactionAmount { Account = Account.AccountsReceivable, Amount = -accountsReceivable });
                amounts.Add(new TransactionAmount { Account = Account.Payments, Amount = -(amount - accountsReceivable) });
            }
            CreateTransaction(timestamp, date, createdByUserId, order, amounts);
            CreatePosting(date, order.User!, PostingType.PayIn, order.Id);
        }

        public void CreateSettlementTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order order, Reservation reservation, int damages, string? description)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));
            if (damages < 0 || damages > reservation.Price!.Deposit)
                throw new ArgumentOutOfRangeException(nameof(damages));
            if (damages > 0 && string.IsNullOrEmpty(description))
                throw new ArgumentException("Value cannot be null or empty.", nameof(description));

            var deposit = reservation.Price.Deposit;
            var amountToRefund = deposit - damages;

            var amounts = new List<TransactionAmount>()
                {
                    new TransactionAmount { Account = Account.Deposits, Amount = deposit },
                };
            if (damages > 0)
                amounts.Add(new TransactionAmount { Account = Account.Damages, Amount = -damages });
            if (amountToRefund > 0)
                amounts.Add(new TransactionAmount { Account = Account.Payments, Amount = -amountToRefund });
            CreateTransaction(timestamp, date, createdByUserId, order, amounts);
        }

        public void CreatePayOutTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order order, int amount)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var amounts = new[]
            {
                    new TransactionAmount { Account = Account.Bank, Amount = -amount },
                    new TransactionAmount { Account = Account.Payments, Amount = amount }
                };
            CreateTransaction(timestamp, date, createdByUserId, order, amounts);
        }

        public void CreatePaymentsUsedTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order orderWithPayments, Order orderWithAccountsReceivable)
        {
            if (orderWithPayments is null)
                throw new ArgumentNullException(nameof(orderWithPayments));
            if (orderWithAccountsReceivable is null)
                throw new ArgumentNullException(nameof(orderWithAccountsReceivable));

            var payments = -orderWithPayments.Balance(Account.Payments);
            var accountsReceivable = orderWithAccountsReceivable.Balance(Account.AccountsReceivable);
            var amount = Math.Min(payments, accountsReceivable);

            var paymentsAmounts = new List<TransactionAmount>()
                {
                    new TransactionAmount { Account = Account.Payments, Amount = amount },
                    new TransactionAmount { Account = Account.ToAccountsReceivable, Amount = -amount }
                };
            CreateTransaction(timestamp, date, createdByUserId, orderWithPayments, paymentsAmounts);

            var accountReceivableAmounts = new List<TransactionAmount>()
                {
                    new TransactionAmount { Account = Account.AccountsReceivable, Amount = -amount },
                    new TransactionAmount { Account = Account.FromPayments, Amount = amount }
                };
            CreateTransaction(timestamp, date, createdByUserId, orderWithAccountsReceivable, accountReceivableAmounts);
        }

        private static void UpdateAccountBalances(User user, Transaction transaction)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            foreach (var amount in transaction.Amounts!)
            {
                var balance = user.AccountBalances.FirstOrDefault(b => b.Account == amount.Account);
                if (balance != null)
                    balance.Amount += amount.Amount;
                else
                    user.AccountBalances!.Add(new AccountBalance { Account = amount.Account, Amount = amount.Amount, User = user });
            }

            ValidateAccountBalances(user);
        }

        public void CreatePosting(LocalDate date, User user, PostingType type, int? orderId = null)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var previousPosting = user.Postings.OrderBy(posting => posting.Id).LastOrDefault();
            var previousIncomeBalance = previousPosting?.IncomeBalance ?? 0;
            var previousBankBalance = previousPosting?.BankBalance ?? 0;
            var previousDepositsBalance = previousPosting?.DepositsBalance ?? 0;
            var currentIncomeBalance = user.Balance(Account.Rent) + user.Balance(Account.Cleaning) + user.Balance(Account.CancellationFees) + user.Balance(Account.Damages);
            var currentBankBalance = user.Balance(Account.Bank);
            var currentDepositsBalance = user.Balance(Account.AccountsReceivable) + user.Balance(Account.Deposits) + user.Balance(Account.Payments);
            var incomeChange = currentIncomeBalance - previousIncomeBalance;
            var bankChange = currentBankBalance - previousBankBalance;
            var depositsChange = currentDepositsBalance - previousDepositsBalance;
            // When making a pay-in that is not enough to cover the rent the
            // deposits change wille be "negative" (being a liability it's
            // actually positive). The posting has to reduce the income part to
            // lower the deposits the be 0. Basically, a too small pay-in is
            // first applied to the rent and then if all rent is covered it's
            // applied to the deposit.
            if (bankChange > 0 && depositsChange > 0)
            {
                incomeChange += depositsChange;
                depositsChange = 0;
                currentIncomeBalance = previousIncomeBalance + incomeChange;
                currentDepositsBalance = previousDepositsBalance;
            }
            var posting = new Posting
            {
                Date = date,
                Type = type,
                UserId = user.Id,
                OrderId = orderId,
                FullName = user.FullName,
                Income = incomeChange,
                Bank = bankChange,
                Deposits = depositsChange,
                IncomeBalance = currentIncomeBalance,
                BankBalance = currentBankBalance,
                DepositsBalance = currentDepositsBalance,
            };
            ValidatePosting(posting);
            user.Postings!.Add(posting);
        }

        private static Transaction CreateTransaction(Instant timestamp, LocalDate date, int createdByUserId, Order order, ICollection<TransactionAmount> amounts)
        {
            var transaction = new Transaction
            {
                Date = date,
                CreatedByUserId = createdByUserId,
                Timestamp = timestamp,
                UserId = order.User!.Id,
                Order = order,
                Amounts = amounts
            };
            ValidateTransaction(transaction);
            order.Transactions!.Add(transaction);
            UpdateAccountBalances(order.User, transaction);
            return transaction;
        }

        [Conditional("DEBUG")]
        private static void ValidateTransaction(Transaction transaction)
        {
            Debug.Assert(transaction.Amounts!.Count > 0);
            Debug.Assert(transaction.Amounts.All(amount => amount.Amount != 0));
            Debug.Assert(transaction.Amounts.Sum(amount => amount.Amount)== 0);
        }

        [Conditional("DEBUG")]
        private static void ValidateAccountBalances(User user)
        {
            // Income
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Rent)?.Amount ?? 0) <= 0);
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Cleaning)?.Amount ?? 0) <= 0);
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.CancellationFees)?.Amount ?? 0) <= 0);
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Damages)?.Amount ?? 0) <= 0);
            // Assets
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Bank)?.Amount ?? 0) >= 0);
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.AccountsReceivable)?.Amount ?? 0) >= 0);
            // Liabilities
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Deposits)?.Amount ?? 0) <= 0);
            Debug.Assert((user.AccountBalances.SingleOrDefault(balance => balance.Account == Account.Payments)?.Amount ?? 0) <= 0);
        }

        [Conditional("DEBUG")]
        private static void ValidatePosting(Posting posting)
        {
            Debug.Assert(posting.IncomeBalance <= 0);
            Debug.Assert(posting.BankBalance >= 0);
            Debug.Assert(posting.DepositsBalance <= 0);
            Debug.Assert(posting.Income + posting.Bank + posting.Deposits == 0);
            Debug.Assert(posting.IncomeBalance + posting.BankBalance + posting.DepositsBalance == 0);
        }
    }
}