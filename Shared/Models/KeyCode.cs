using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class KeyCode
    {
        public int ResourceId { get; set; }
        public LocalDate Date { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}