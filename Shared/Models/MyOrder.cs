﻿using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class MyOrder
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public Instant CreatedTimestamp { get; set; }
        public IEnumerable<Reservation>? Reservations { get; set; }
        public bool CanBeEdited { get; set; }
        public OrderTotals? Totals { get; set; }
    }
}