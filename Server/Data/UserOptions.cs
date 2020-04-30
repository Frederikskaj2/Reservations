using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class UserOptions
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public EmailSubscriptions EmailSubscriptions { get; set; }
        public string? Password { get; set; }
    }
}
