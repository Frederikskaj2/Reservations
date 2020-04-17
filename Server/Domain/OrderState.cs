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
            { Account.PayIns, 0 },
            { Account.PayOuts, 0 }
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
                    accountBalances[Account.PayIns] += transaction.Amount;
                    CreatePayInPostings(transaction);
                    break;
                case TransactionType.PayOut:
                    accountBalances[Account.PayOuts] += transaction.Amount;
                    CreatePayOutPostings(transaction);
                    break;
                default:
                    throw new ArgumentException("Invalid transaction type.", nameof(transaction));
            }
        }

        private void CreatePayInPostings(Transaction transaction)
        {
            var date = transaction.Timestamp.InZone(dateTimeZone).Date;

            var localPostings = new List<Posting>
            {
                // Amount paid by the user.
                new Posting
                {
                    TransactionId = transaction.Id,
                    Date = date,
                    OrderId = orderId,
                    Account = Account.PayIns,
                    Amount = transaction.Amount
                }
            };

            var payIn = transaction.Amount;
            var income = -accountBalances[Account.Income];

            var incomeConsumed = Math.Min(income, payIn);
            Debug.Assert(incomeConsumed > 0);
            // Rent.
            localPostings.Add(
                new Posting
                {
                    TransactionId = transaction.Id,
                    Date = date,
                    OrderId = orderId,
                    Account = Account.Income,
                    Amount = -incomeConsumed
                });
            accountBalances[Account.Income] += incomeConsumed;
            payIn -= incomeConsumed;

            if (payIn > 0)
            {
                // The deposit (and any excess amount paid by the user).
                localPostings.Add(
                    new Posting
                    {
                        TransactionId = transaction.Id,
                        Date = date,
                        OrderId = orderId,
                        Account = Account.Deposits,
                        Amount = -payIn
                    });
                accountBalances[Account.Deposits] += payIn;
            }

#if DEBUG
            Debug.Assert(localPostings.All(posting => posting.Amount != 0));
            var debit = localPostings.Where(posting => posting.Amount > 0).Sum(posting => posting.Amount);
            var credit = localPostings.Where(posting => posting.Amount < 0).Sum(posting => -posting.Amount);
            Debug.Assert(debit == credit);
#endif
            postings.AddRange(localPostings);
        }

        private void CreatePayOutPostings(Transaction transaction)
        {
            var date = transaction.Timestamp.InZone(dateTimeZone).Date;

            var localPostings = new List<Posting>
            {
                // Amount paid to the user.
                new Posting
                {
                    TransactionId = transaction.Id,
                    Date = date,
                    OrderId = orderId,
                    Account = Account.PayOuts,
                    Amount = transaction.Amount
                }
            };

            var payOut = -transaction.Amount;
            var income = -accountBalances[Account.Income];

            if (income >= 0)
            {
                if (income > 0)
                {
                    // Damages.
                    localPostings.Add(
                        new Posting
                        {
                            TransactionId = transaction.Id,
                            Date = date,
                            OrderId = orderId,
                            Account = Account.Income,
                            Amount = -income
                        });
                    accountBalances[Account.Income] += income;
                }

                var deposits = income + payOut;
                // Deposit being refunded.
                localPostings.Add(
                    new Posting
                    {
                        TransactionId = transaction.Id,
                        Date = date,
                        OrderId = orderId,
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
                localPostings.Add(
                    new Posting
                    {
                        TransactionId = transaction.Id,
                        Date = date,
                        OrderId = orderId,
                        Account = Account.Income,
                        Amount = refunded
                    });
                accountBalances[Account.Income] -= refunded;
                payOut -= refunded;

                if (payOut > 0)
                {
                    // Deposit being refunded.
                    localPostings.Add(
                        new Posting
                        {
                            TransactionId = transaction.Id,
                            Date = date,
                            OrderId = orderId,
                            Account = Account.Deposits,
                            Amount = payOut
                        });
                    accountBalances[Account.Deposits] -= payOut;
                }
            }

#if DEBUG
            Debug.Assert(localPostings.All(posting => posting.Amount != 0));
            var debit = localPostings.Where(posting => posting.Amount > 0).Sum(posting => posting.Amount);
            var credit = localPostings.Where(posting => posting.Amount < 0).Sum(posting => -posting.Amount);
            Debug.Assert(debit == credit);
#endif
            postings.AddRange(localPostings);

        }
    }
}