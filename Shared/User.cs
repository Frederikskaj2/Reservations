namespace Frederikskaj2.Reservations.Shared
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        public bool IsAdministrator { get; set; }
    }
}