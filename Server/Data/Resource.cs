using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Resource
    {
        public int Id { get; set; }
        public virtual ICollection<KeyCode>? KeyCodes { get; set; }
        public int Sequence { get; set; }
        public ResourceType Type { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}