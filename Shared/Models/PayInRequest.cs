using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class PayInRequest
    {
        public LocalDate Date { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Oplys kontonummer hvortil depositum kan udbetales")]
        [StringLength(50, ErrorMessage = "Kontonummeret er for langt")]
        [RegularExpression(
            @"^\s*[0-9]{4}-[0-9]{4,}\s*$", ErrorMessage = "Angiv et kontonummer på formen 1111-2222333344 - start med registreringsnummer")]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "Angiv et beløb større end nul.")]
        public int Amount { get; set; }
    }
}
