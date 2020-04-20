using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class CleaningTask
    {
        public CleaningTask(LocalDate date, string resourceName, int resourceSequence)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));

            Date = date;
            ResourceName = resourceName;
            ResourceSequence = resourceSequence;
        }

        public LocalDate Date { get; }
        public string ResourceName { get; }
        public int ResourceSequence { get; }
    }
}
