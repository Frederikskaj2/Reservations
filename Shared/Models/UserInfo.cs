namespace Frederikskaj2.Reservations.Shared
{
    public class UserInfo
    {
        public string? Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAdministrator { get; set; }
        public int? ApartmentId { get; set; }
    }
}