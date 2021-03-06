﻿using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SignInRequest
    {
        [Required(ErrorMessage = "Angiv din email")]
        [EmailAddress(ErrorMessage = "Angiv en korrekt email")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Angiv din adgangskode")]
        public string Password { get; set; } = string.Empty;

        public bool IsPersistent { get; set; }
    }
}