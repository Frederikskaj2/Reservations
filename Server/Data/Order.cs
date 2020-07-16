using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Entity Framework require an accesible setter.")]
    public class Order
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public OrderFlags Flags { get; set; }
        public int? ApartmentId { get; set; }
        public virtual Apartment? Apartment { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public virtual ICollection<Reservation>? Reservations { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }

        [Timestamp]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This property is only used by the framework.")]
        public byte[]? Timestamp { get; set; }
    }
}