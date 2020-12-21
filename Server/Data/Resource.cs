using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Entity Framework require an accessible setter.")]
    public class Resource
    {
        public int Id { get; set; }
        public virtual ICollection<LockBoxCode>? LockBoxCodes { get; set; }
        public int Sequence { get; set; }
        public ResourceType Type { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}