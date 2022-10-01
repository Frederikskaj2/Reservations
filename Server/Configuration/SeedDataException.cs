using System;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.Server;

[Serializable]
public class SeedDataException : Exception
{
    public SeedDataException() { }

    public SeedDataException(string message) : base(message) { }

    public SeedDataException(string message, Exception inner) : base(message, inner) { }

    protected SeedDataException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}