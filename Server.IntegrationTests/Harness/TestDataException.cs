using System;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

[Serializable]
public class TestDataException : Exception
{
    public TestDataException() { }

    public TestDataException(string message) : base(message) { }

    public TestDataException(string message, Exception inner) : base(message, inner) { }

    protected TestDataException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}