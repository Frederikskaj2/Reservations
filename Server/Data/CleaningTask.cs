﻿using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class CleaningTask
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource? Resource { get; set; }
        public LocalDate Date { get; set; }
    }
}
