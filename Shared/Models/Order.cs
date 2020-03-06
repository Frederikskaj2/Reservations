using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Apartment? Apartment { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public List<Reservation>? Reservations { get; set; }
    }
}