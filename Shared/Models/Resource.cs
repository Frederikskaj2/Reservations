using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class Resource
    {
        public int Id { get; set; }
        public int Sequence { get; set; }
        public ResourceType Type { get; set; }
        public string? Name { get; set; }
    }
}