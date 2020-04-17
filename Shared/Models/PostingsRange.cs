using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class PostingsRange
    {
        public LocalDate EarliestMonth { get; set; }
        public LocalDate LatestMonth { get; set; }
    }
}