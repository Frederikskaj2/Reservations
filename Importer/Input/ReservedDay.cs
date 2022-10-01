using NodaTime;

namespace Frederikskaj2.Reservations.Importer.Input;

public class ReservedDay
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public virtual Reservation? Reservation { get; set; }
    public LocalDate Date { get; set; }
    public int ResourceId { get; set; }
    public virtual Resource? Resource { get; set; }
}
