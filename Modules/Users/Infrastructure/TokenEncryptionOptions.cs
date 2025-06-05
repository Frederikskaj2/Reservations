using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public class TokenEncryptionOptions
{
    public const int KeyLength = 32;
    public string Key { get; init; } = "";
    public string ConfirmEmailPurpose { get; init; } = "Email";
    public Duration ConfirmEmailDuration { get; init; } = Duration.FromDays(7);
    public string NewPasswordPurpose { get; init; } = "Password";
    public Duration NewPasswordDuration { get; init; } = Duration.FromDays(1);
}
