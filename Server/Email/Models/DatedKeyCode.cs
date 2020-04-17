using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class DatedKeyCode
    {
        public DatedKeyCode(LocalDate date, string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Value cannot be null or empty.", nameof(code));

            Date = date;
            Code = code;
        }

        public LocalDate Date { get; }
        public string Code { get; }
    }
}