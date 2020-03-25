using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class UpdatePasswordRequest
    {
        [Required(ErrorMessage = "Angiv din nuværende adgangskode")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Du skal vælge en ny adgangskode")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare(nameof(NewPassword), ErrorMessage = "Den nye adgangskode er ikke den samme")]
        public string? ConfirmNewPassword { get; set; }
    }
}