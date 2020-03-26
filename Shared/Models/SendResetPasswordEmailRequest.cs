using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SendResetPasswordEmailRequest
    {
        [Required(ErrorMessage = "Angiv din email")]
        [EmailAddress(ErrorMessage = "Angiv en korrekt email")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        public string Email { get; set; } = string.Empty;
    }
}