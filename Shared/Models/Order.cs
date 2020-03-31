using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public IEnumerable<Reservation>? Reservations { get; set; }
        public OrderTotals? Totals { get; set; }
    }
}