﻿using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SignUpRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv din email")]
        [EmailAddress(ErrorMessage = "Angiv en korrekt email")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit fulde navn")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*(?<name>(?:\p{L}|\p{P})+(?:\s+(?:\p{L}|\p{P})+)+)\s*$", ErrorMessage = "Angiv dit fulde navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit telefonnummer")]
        [StringLength(50, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", ErrorMessage = "Angiv et korrekt telefonnummer")]
        public string Phone { get; set; } = string.Empty;

        public int? ApartmentId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Du skal vælge en adgangskode")]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Adgangskoden er ikke den samme")]
        public string? ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Du skal samtykke til at vi behandler dine personoplysninger")]
        public bool DidConsent { get; set; }

        public bool IsPersistent { get; set; }
    }
}