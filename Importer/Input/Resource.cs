using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Importer.Input;

public class Resource
{
    public int Id { get; set; }
    public virtual ICollection<LockBoxCode>? LockBoxCodes { get; set; }
    public int Sequence { get; set; }
    public ResourceType Type { get; set; }
    public string Name { get; set; } = string.Empty;
}