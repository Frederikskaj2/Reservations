namespace Frederikskaj2.Reservations.Users;

public class PasswordOptions
{
    public PasswordPolicyOptions Policy { get; init; } = new();
    public RemotePasswordCheckerOptions RemoteChecker { get; init; } = new();
    public PasswordHashingOptions Hashing { get; init; } = new();
}
