using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
        public string? AccountNumber { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public List<Reservation>? Reservations { get; set; }
        public Price? Price { get; set; }
    }
}