namespace Frederikskaj2.Reservations.Server.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? HashedPassword { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsPendingDelete { get; set; }
        public int? ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
    }
}