using System;
using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class AccountAmount
    {
        public AccountAmount(Account account, int amount)
        {
            Account = account;
            Amount = amount;
        }

        public Account Account { get; }
        // Debit is positive, credit is negative.
        public int Amount { get; }
    }

}