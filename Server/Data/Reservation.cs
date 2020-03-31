﻿using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource? Resource { get; set; }
        public ReservationStatus Status { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public virtual ICollection<ReservedDay>? Days { get; set; }
        public Price? Price { get; set; }

        public bool CanBeCancelled(LocalDate today, ReservationsOptions options)
            => Status == ReservationStatus.Reserved
               || Status == ReservationStatus.Confirmed
               && today.PlusDays(options.MinimumCancellationNoticeInDays) <= Days.Min(day => day.Date);
    }
}