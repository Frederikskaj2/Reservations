namespace Frederikskaj2.Reservations.Application;

public class PasswordPolicyOptions
{
    public int MinimumLength { get; init; } = 5;
    public int MaximumExposedCount { get; init; } = 10;
}
