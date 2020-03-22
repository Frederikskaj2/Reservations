using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public Reservation(
            int id, int resourceId, ReservationStatus status, Instant updatedTimestamp, Price price, LocalDate date,
            int durationInDays, bool canBeCancelled)
        {
            Id = id;
            ResourceId = resourceId;
            Status = status;
            UpdatedTimestamp = updatedTimestamp;
            Price = price;
            Date = date;
            DurationInDays = durationInDays;
            CanBeCancelled = canBeCancelled;
        }

        public int Id { get; }
        public int ResourceId { get; }
        public ReservationStatus Status { get; }
        public Instant UpdatedTimestamp { get; }
        public Price Price { get; }
        public LocalDate Date { get; }
        public int DurationInDays { get; }
        public bool CanBeCancelled { get; }
    }
}