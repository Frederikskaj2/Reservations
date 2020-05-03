using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class PaymentRequest
    {
        public LocalDate Date { get; set; }

        [Range(1, 100000, ErrorMessage = "Angiv et beløb større end nul.")]
        public int Amount { get; set; }
    }
}
