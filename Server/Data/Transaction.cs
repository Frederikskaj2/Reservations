using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Transaction
    {
        public int Id { get; set; }
        public LocalDate Date { get; set; }
        public int CreatedByUserId { get; set; }
        public Instant Timestamp { get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public int? ResourceId { get; set; }
        public LocalDate? ReservationDate { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<TransactionAmount>? Amounts { get; set; }
    }
}
