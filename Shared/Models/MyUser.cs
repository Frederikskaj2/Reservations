namespace Frederikskaj2.Reservations.Shared
{
    public class MyUser
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public Apartment? Apartment { get; set; }
    }
}