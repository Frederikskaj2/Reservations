using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SignInRequest
    {
        [Required(ErrorMessage = "Angiv din email")]
        [EmailAddress(ErrorMessage = "Angiv en korrekt email")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Angiv din adgangskode")]
        public string? Password { get; set; }

        public bool IsPersistent { get; set; }
    }
}