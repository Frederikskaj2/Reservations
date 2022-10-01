using System;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.Application;

[Serializable]
public class InternalValidationException : Exception
{
    public InternalValidationException() { }
        
    public InternalValidationException(string message) : base(message) { }
        
    public InternalValidationException(string message, Exception inner) : base(message, inner) { }

    protected InternalValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}