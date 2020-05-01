using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class CleaningTask
    {
        public LocalDate Date { get; set; }
        public int ResourceId { get; set; }
    }
}