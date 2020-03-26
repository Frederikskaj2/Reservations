using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class NewPasswordRequest
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Du skal vælge en ny adgangskode")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare(nameof(NewPassword), ErrorMessage = "Den nye adgangskode er ikke den samme")]
        public string? ConfirmNewPassword { get; set; }
    }
}