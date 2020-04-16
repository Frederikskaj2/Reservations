using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class KeyCode
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource? Resource { get; set; }
        public LocalDate Date { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
