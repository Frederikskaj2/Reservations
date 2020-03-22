using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public Order(
            int id, string accountNumber, Instant createdTimestamp, IEnumerable<Reservation> reservations,
            bool canBeEdited)
        {
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));

            Id = id;
            AccountNumber = accountNumber;
            CreatedTimestamp = createdTimestamp;
            Reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
            CanBeEdited = canBeEdited;
        }

        public int Id { get; }
        public string AccountNumber { get; }
        public Instant CreatedTimestamp { get; }
        public IEnumerable<Reservation> Reservations { get; }
        public bool CanBeEdited { get; }
    }
}