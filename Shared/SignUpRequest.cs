using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SignUpRequest
    {
        [Required(ErrorMessage = "Angiv din email")]
        [EmailAddress(ErrorMessage = "Angiv en korrekt email")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Angiv dit fulde navn")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*(?<name>\p{L}+(\s+\p{L}+)+)\s*$", ErrorMessage = "Angiv dit fulde navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du skal vælge en adgangskode")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Adgangskoden er ikke den samme")]
        public string? ConfirmPassword { get; set; }

        public bool IsPersistent { get; set; }

        public string? RedirectUri { get; set; }
    }
}