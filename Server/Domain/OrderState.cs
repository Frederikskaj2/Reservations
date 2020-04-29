using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal class OrderState
    {
        public OrderState(int orderId, DateTimeZone dateTimeZone)
        {
            this.orderId = orderId;
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        private readonly Dictionary<Account, int> accountBalances = new Dictionary<Account, int>
        {
            { Account.Income, 0 },
            { Account.Deposits, 0 },
            { Account.Bank, 0 }
        };

        private readonly DateTimeZone dateTimeZone;
        private readonly int orderId;
        private readonly List<Posting> postings = new List<Posting>();

        public IEnumerable<Posting> Postings => postings;

        public void ApplyTransaction(Transaction transaction)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));
            if (transaction.OrderId != orderId)
                throw new ArgumentException("Invalid order ID.", nameof(transaction));

            switch (transaction.Type)
            {
                case TransactionType.Order:
                case TransactionType.OrderCancellation:
                case TransactionType.CancellationFee:
                case TransactionType.SettlementDamages:
                    accountBalances[Account.Income] += transaction.Amount;
                    break;
                case TransactionType.Deposit:
                case TransactionType.DepositCancellation:
                case TransactionType.SettlementDeposit:
                    accountBalances[Account.Deposits] += transaction.Amount;
                    break;
                case TransactionType.PayIn:
                    accountBalances[Account.Bank] += transaction.Amount;
                    CreatePayInPostings(transaction);
                    break;
                case TransactionType.PayOut:
                    accountBalances[Account.Bank] += transaction.Amount;
                    CreatePayOutPostings(transaction);
                    break;
                default:
                    throw new ArgumentException("Invalid transaction type.", nameof(transaction));
            }
        }

        private void CreatePayInPostings(Transaction transaction)
        {
            var date = transaction.Timestamp.InZone(dateTimeZone).Date;

            var accountAmounts = new List<AccountAmount>();
            var posting = new Posting
            {
                Date = date,
                OrderId = orderId,
                AccountAmounts = accountAmounts
            };

            // Amount paid by the user.
            accountAmounts.Add(
                new AccountAmount
                {
                    Account = Account.Bank,
                    Amount = transaction.Amount
                });

            var payIn = transaction.Amount;
            var income = -accountBalances[Account.Income];

            var incomeConsumed = Math.Min(income, payIn);
            Debug.Assert(incomeConsumed > 0);
            // Rent.
            accountAmounts.Add(
                new AccountAmount
                {
                    Account = Account.Income,
                    Amount = -incomeConsumed
                });
            accountBalances[Account.Income] += incomeConsumed;
            payIn -= incomeConsumed;

            if (payIn > 0)
            {
                // The deposit (and any excess amount paid by the user).
                accountAmounts.Add(
                    new AccountAmount
                    {
                        Account = Account.Deposits,
                        Amount = -payIn
                    });
                accountBalances[Account.Deposits] += payIn;
            }

            ValidatePosting(posting);

            postings.Add(posting);
        }

        private void CreatePayOutPostings(Transaction transaction)
        {
            var date = transaction.Timestamp.InZone(dateTimeZone).Date;

            var accountAmounts = new List<AccountAmount>();
            var posting = new Posting
            {
                Date = date,
                OrderId = orderId,
                AccountAmounts = accountAmounts
            };

            // Amount paid to the user.
            accountAmounts.Add(
                new AccountAmount
                {
                    Account = Account.Bank,
                    Amount = transaction.Amount
                });

            var payOut = -transaction.Amount;
            var income = -accountBalances[Account.Income];

            if (income >= 0)
            {
                if (income > 0)
                {
                    // Damages.
                    accountAmounts.Add(
                        new AccountAmount
                        {
                            Account = Account.Income,
                            Amount = -income
                        });
                    accountBalances[Account.Income] += income;
                }

                var deposits = income + payOut;
                // Deposit being refunded.
                accountAmounts.Add(
                    new AccountAmount
                    {
                        Account = Account.Deposits,
                        Amount = deposits
                    });
                accountBalances[Account.Deposits] -= deposits;
            }
            else if (income < 0)
            {
                var refunded = Math.Min(-income, payOut);
                Debug.Assert(refunded > 0);
                // Rent being refunded.
                accountAmounts.Add(
                    new AccountAmount
                    {
                        Account = Account.Income,
                        Amount = refunded
                    });
                accountBalances[Account.Income] -= refunded;
                payOut -= refunded;

                if (payOut > 0)
                {
                    // Deposit being refunded.
                    accountAmounts.Add(
                        new AccountAmount
                        {
                            Account = Account.Deposits,
                            Amount = payOut
                        });
                    accountBalances[Account.Deposits] -= payOut;
                }
            }

            ValidatePosting(posting);

            postings.Add(posting);
        }

        [Conditional("DEBUG")]
        private static void ValidatePosting(Posting posting)
        {
            Debug.Assert(posting.AccountAmounts.All(accountAmount => accountAmount.Amount != 0));
            Debug.Assert(
                posting.AccountAmounts.GroupBy(accountAmount => accountAmount.Account)
                    .All(grouping => grouping.Count() == 1));
            var debit = posting.AccountAmounts.Where(accountAmount => accountAmount.Amount > 0)
                .Sum(accountAmount => accountAmount.Amount);
            var credit = posting.AccountAmounts.Where(accountAmount => accountAmount.Amount < 0)
                .Sum(accountAmount => -accountAmount.Amount);
            Debug.Assert(debit == credit);
        }
    }
}