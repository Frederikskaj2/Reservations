using System;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public class SeedDataException : Exception
{
    public SeedDataException() { }

    public SeedDataException(string message) : base(message) { }

    public SeedDataException(string message, Exception inner) : base(message, inner) { }
}
