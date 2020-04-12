using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AccountNumber { get; set; }
        public IEnumerable<Reservation>? Reservations { get; set; }
        public OrderTotals? Totals { get; set; }
    }
}