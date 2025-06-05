using System;

namespace Frederikskaj2.Reservations.LockBox;

public class LockBoxCodesException : Exception
{
    public LockBoxCodesException() { }

    public LockBoxCodesException(string message) : base(message) { }

    public LockBoxCodesException(string message, Exception innerException) : base(message, innerException) { }
}
