using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class Resource
    {
        public Resource(int id, int sequence, ResourceType type, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            Id = id;
            Sequence = sequence;
            Type = type;
            Name = name;
        }

        public int Id { get; }
        public int Sequence { get; }
        public ResourceType Type { get; }
        public string Name { get; }
    }
}