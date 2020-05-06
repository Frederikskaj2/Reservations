using System;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class Posting
    {
        public Posting(LocalDate date, PostingType type, string fullName, int? orderId, int income, int bank, int deposits)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(fullName));

            Date = date;
            Type  =  type;
            FullName = fullName;
            OrderId = orderId;
            Income = income;
            Bank = bank;
            Deposits = deposits;
        }

        public LocalDate Date { get; }
        public PostingType Type { get; }
        public string FullName { get; }
        public int? OrderId { get; }
        public int Income { get; }
        public int Bank { get; }
        public int Deposits { get; }
    }
}