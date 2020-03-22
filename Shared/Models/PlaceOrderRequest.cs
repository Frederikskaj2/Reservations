﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class PlaceOrderRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Du skal oplyse din adresse")]
        public int ApartmentId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Oplys kontonummer hvortil depositum kan udbetales")]
        [StringLength(50, ErrorMessage = "Kontonummeret er for langt")]
        [RegularExpression(
            @"^\s*(?:[0-9]{4}(?:[- 0-9]+)*|[a-zA-Z]{2}[0-9]{2}(?: ?[0-9a-zA-Z]{4})*(?: ?[0-9a-zA-Z]{1,4}))\s*$",
            ErrorMessage = "Angiv et kontonummer (inklusiv registreringsnummer) eller et IBAN-nummer")]
        public string AccountNumber { get; set; } = string.Empty;

        public List<ReservationRequest> Reservations { get; } = new List<ReservationRequest>();
    }
}