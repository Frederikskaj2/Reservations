using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class LockBoxCodeOptions
    {
        public LocalDate Date { get; set; }
        public string? Code { get; set; }
    }
}