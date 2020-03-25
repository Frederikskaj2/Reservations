using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class SeedDataOptions
    {
        public IEnumerable<UserOptions>? Users { get; set; }
        public IEnumerable<ResourceOptions>? Resources { get; set; }
    }
}