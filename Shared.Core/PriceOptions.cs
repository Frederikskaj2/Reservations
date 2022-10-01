namespace Frederikskaj2.Reservations.Shared.Core;

public class PriceOptions
{
    public Amount LowRentPerNight { get; init; }
    public Amount HighRentPerNight { get; init; }
    public Amount Cleaning { get; init; }
    public Amount LowDeposit { get; init; }
    public Amount HighDeposit { get; init; }
}
