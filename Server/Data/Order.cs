using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Order
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public int? ApartmentId { get; set; }
        public virtual Apartment? Apartment { get; set; }
        public string? AccountNumber { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public virtual ICollection<Reservation>? Reservations { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}