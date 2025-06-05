using System;

namespace Frederikskaj2.Reservations.Core;

public class InternalValidationException : Exception
{
    public InternalValidationException() { }

    public InternalValidationException(string message) : base(message) { }

    public InternalValidationException(string message, Exception inner) : base(message, inner) { }
}
