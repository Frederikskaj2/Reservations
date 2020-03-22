using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Holiday
    {
        public Holiday(int id, LocalDate date)
        {
            Id = id;
            Date = date;
        }

        public int Id { get; }
        public LocalDate Date { get; }
    }
}