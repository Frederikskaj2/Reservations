using System;

namespace Frederikskaj2.Reservations.Shared
{
    [Flags]
    public enum EmailSubscriptions
    {
        None,
        Order,
        Settlement = Order << 1,
        Cleaning = Settlement << 1
    }
}