using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class DatedLockBoxCode
    {
        public LocalDate Date { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}