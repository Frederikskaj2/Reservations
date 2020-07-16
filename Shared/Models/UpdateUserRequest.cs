using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared
{
    public class UpdateUserRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv brugerens fulde navn")]
        [StringLength(100, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*(?<name>(?:\p{L}|\p{P})+(?:\s+(?:\p{L}|\p{P})+)+)\s*$", ErrorMessage = "Angiv brugerens fulde navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Angiv brugerens telefonnummer")]
        [StringLength(50, ErrorMessage = "Teksten er for lang")]
        [RegularExpression(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", ErrorMessage = "Angiv et korrekt telefonnummer")]
        public string Phone { get; set; } = string.Empty;

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "A public setter is required for serialization.")]
        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        public bool IsPendingDelete { get; set; }
    }
}