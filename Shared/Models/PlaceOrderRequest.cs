using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class PlaceOrderRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit fulde navn")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*(?<name>\p{L}+(\s+\p{L}+)+)\s*$", ErrorMessage = "Angiv dit fulde navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit telefonnummer")]
        [StringLength(50, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", ErrorMessage = "Angiv et korrekt telefonnummer")]
        public string Phone { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Du skal oplyse din adresse")]
        public int ApartmentId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Oplys kontonummer hvortil depositum kan udbetales")]
        [StringLength(50, ErrorMessage = "Kontonummeret er for langt")]
        [RegularExpression(
            @"^\s*(?:[0-9]{4}(?:[- 0-9]+)*|[a-zA-Z]{2}[0-9]{2}(?: ?[0-9a-zA-Z]{4})*(?: ?[0-9a-zA-Z]{1,4}))\s*$",
            ErrorMessage = "Angiv et kontonummer (inklusiv registreringsnummer) eller et IBAN-nummer")]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "Du skal acceptere betingelserne")]
        public bool DidAcceptTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Du skal samtykke til at vi behandler dine personoplysninger")]
        public bool DidConsent { get; set; }

        public List<ReservationRequest> Reservations { get; } = new List<ReservationRequest>();
    }
}