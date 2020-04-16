using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class DatedKeyCode
    {
        public LocalDate Date { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}