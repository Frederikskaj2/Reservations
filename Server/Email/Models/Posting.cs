using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class Posting
    {
        public Posting(LocalDate date, int orderId, IEnumerable<AccountAmount> accountAmounts)
        {
            Date = date;
            OrderId = orderId;
            AccountAmounts = accountAmounts ?? throw new ArgumentNullException(nameof(accountAmounts));
        }

        public LocalDate Date { get; }
        public int OrderId { get; }
        public IEnumerable<AccountAmount> AccountAmounts { get; }
    }
}