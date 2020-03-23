using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public static readonly Order EmptyOrder = new Order();

        public int Id { get; set; }
        public string? AccountNumber { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public IEnumerable<Reservation>? Reservations { get; set; }
        public bool CanBeEdited { get; set; }
    }
}