using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class CleaningTask
    {
        public CleaningTask(LocalDate date, string resourceName, int resourceSequence, string keyCode)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));
            if (string.IsNullOrEmpty(keyCode))
                throw new ArgumentException("Value cannot be null or empty.", nameof(keyCode));

            Date = date;
            ResourceName = resourceName;
            ResourceSequence = resourceSequence;
            KeyCode = keyCode;
        }

        public LocalDate Date { get; }
        public string ResourceName { get; }
        public int ResourceSequence { get; }
        public string KeyCode { get; }
    }
}
