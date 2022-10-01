using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class PasswordErrors
{
    [Parameter] public bool GeneralError { get; set; }
    [Parameter] public bool WrongPassword { get; set; }
    [Parameter] public bool TooShortPassword { get; set; }
    [Parameter] public bool ExposedPassword { get; set; }
    [Parameter] public bool EmailSameAsPassword { get; set; }
}