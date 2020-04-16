using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class Resource
    {
        public Resource(int id, int sequence, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            Id = id;
            Sequence = sequence;
            Name = name;
        }

        public int Id { get; }
        public int Sequence { get; }
        public string Name { get; }
    }
}