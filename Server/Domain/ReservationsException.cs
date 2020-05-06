using System;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.Server.Domain
{
    [Serializable]
    public class ReservationsException : Exception
    {
        public ReservationsException() : base()
        {
        }

        public ReservationsException(string message) : base(message)
        {
        }

        public ReservationsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReservationsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}