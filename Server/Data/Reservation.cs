using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
        public LocalDate Date { get; set; }
        public int DurationInDays { get; set; }
        public virtual ICollection<ReservedDay>? Days { get; set; }
        public Price? Price { get; set; }
        public ReservationEmails SentEmails { get; set; }

        [Timestamp]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This property is only used by the framework.")]
        public byte[]? Timestamp { get; set; }

        public bool CanBeCancelledByAdministrator() => Status == ReservationStatus.Reserved || Status == ReservationStatus.Confirmed;

        public bool CanBeCancelledUser(LocalDate today, ReservationsOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return Status == ReservationStatus.Reserved
                || Status == ReservationStatus.Confirmed
                && today.PlusDays(options.MinimumCancellationNoticeInDays) <= Days.Min(day => day.Date);
        }
    }
}