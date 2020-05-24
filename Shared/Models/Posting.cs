using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Posting
    {
        public LocalDate Date { get; set; }
        public PostingType Type { get; set; }
        public string? FullName { get; set; }
        public int? OrderId { get; set; }
        public int Income { get; set; }
        public int Bank { get; set; }
        public int Deposits { get; set; }
    }
}
