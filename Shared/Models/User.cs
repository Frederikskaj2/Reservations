using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsPendingDelete { get; set; }
        public Apartment? Apartment { get; set; }
    }
}