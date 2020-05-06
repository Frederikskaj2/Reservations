using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared
{
    public class MyUser
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public EmailSubscriptions EmailSubscriptions { get; set; }
        public bool IsPendingDelete { get; set; }
        public int? ApartmentId { get; set; }
        public string? AccountNumber { get; set; }
    }
}