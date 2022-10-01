using System;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.Application;

[Serializable]
public class LockBoxCodesException : Exception
{
    public LockBoxCodesException()
    {
    }

    public LockBoxCodesException(string message) : base(message)
    {
    }

    public LockBoxCodesException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected LockBoxCodesException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
    }
}