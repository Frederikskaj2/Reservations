using NodaTime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Importer.Input;

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