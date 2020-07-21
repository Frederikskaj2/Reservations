namespace Frederikskaj2.Reservations.Shared
{
    public class LockBoxCode
    {
        public int ResourceId {get; set; }
        public string Code { get; set; } = string.Empty;
        public string Difference { get; set; } = string.Empty;
    }
}
