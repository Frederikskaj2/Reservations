using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservedDay
    {
        public ReservedDay(LocalDate date, int resourceId, bool isMyReservation)
        {
            Date = date;
            ResourceId = resourceId;
            IsMyReservation = isMyReservation;
        }

        public LocalDate Date { get; }
        public int ResourceId { get; }
        public bool IsMyReservation { get; }
    }
}