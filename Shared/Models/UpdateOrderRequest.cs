using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class UpdateOrderRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Oplys kontonummer hvortil depositum kan udbetales")]
        [StringLength(50, ErrorMessage = "Kontonummeret er for langt")]
        [RegularExpression(
            @"^\s*[0-9]{4}-[0-9]{4,}\s*$", ErrorMessage = "Angiv et kontonummer på formen 1111-2222333344 - start med registreringsnummer")]
        public string AccountNumber { get; set; } = string.Empty;

        public HashSet<int> CancelledReservations { get; } = new HashSet<int>();

        public bool WaiveFee { get; set; }
    }
}