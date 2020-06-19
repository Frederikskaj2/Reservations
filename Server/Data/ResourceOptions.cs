using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class ResourceOptions
    {
        public ResourceType Type { get; set; }
        public string? Name { get; set; }
        public IEnumerable<KeyCodeOptions>? KeyCodes { get; set; }
    }
}