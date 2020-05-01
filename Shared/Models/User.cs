using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public bool IsPendingDelete { get; set; }
        public int OrderCount { get; set; }
    }
}