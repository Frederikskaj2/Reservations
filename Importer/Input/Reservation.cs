using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Importer.Input;

public class Reservation
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public int ResourceId { get; set; }
    public virtual Resource? Resource { get; set; }
    public ReservationStatus Status { get; set; }
    public Instant UpdatedTimestamp { get; set; }
    public LocalDate Date { get; set; }
    public int DurationInDays { get; set; }
    public virtual ICollection<ReservedDay>? Days { get; set; }
    public Price? Price { get; set; }
    public ReservationEmails SentEmails { get; set; }
}
