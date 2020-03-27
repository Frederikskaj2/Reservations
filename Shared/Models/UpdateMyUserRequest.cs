using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class UpdateMyUserRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit fulde navn")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*(?<name>\p{L}+(\s+\p{L}+)+)\s*$", ErrorMessage = "Angiv dit fulde navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv dit telefonnummer")]
        [StringLength(50, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", ErrorMessage = "Angiv et korrekt telefonnummer")]
        public string Phone { get; set; } = string.Empty;
    }
}