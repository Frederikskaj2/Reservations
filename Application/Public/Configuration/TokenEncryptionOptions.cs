using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public class TokenEncryptionOptions
{
    public const int KeyLength = 32;
    public string Key { get; set; } = "";
    public string ConfirmEmailPurpose { get; set; } = "Email";
    public Duration ConfirmEmailDuration { get; set; } = Duration.FromDays(7);
    public string NewPasswordPurpose { get; set; } = "Password";
    public Duration NewPasswordDuration { get; set; } = Duration.FromDays(1);
}