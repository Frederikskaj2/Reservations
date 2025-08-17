using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public class OrderingOptions
{
    public IReadOnlyDictionary<ResourceType, PriceOptions> Prices { get; init; } = new Dictionary<ResourceType, PriceOptions>
    {
        {
            ResourceType.BanquetFacilities, new()
            {
                LowRentPerNight = 500,
                HighRentPerNight = 750,
                Cleaning = 500,
                LowDeposit = 1000,
                HighDeposit = 3000,
            }
        },
        {
            ResourceType.Bedroom, new()
            {
                LowRentPerNight = 200,
                HighRentPerNight = 250,
                Cleaning = 200,
                LowDeposit = 500,
                HighDeposit = 500,
            }
        },
    };

    public int ReservationIsNotAllowedBeforeDaysFromNow { get; init; } = 5;
    public int ReservationIsNotAllowedAfterDaysFromNow { get; init; } = 274;
    // The key code logic expects that at most two key codes are required for a single reservation.
    public int MaximumAllowedReservationNights { get; init; } = 7;
    public LocalTime CheckInTime { get; init; } = new(12, 0);
    public LocalTime CheckOutTime { get; init; } = new(10, 0);
    public Amount CancellationFee { get; init; } = 200;
    public int MinimumCancellationNoticeInDays { get; init; } = 5;
    public Duration CancellationWithoutFeeDuration { get; init; } = Duration.FromDays(5);
    public string PayInAccountNumber { get; init; } = "9444-12501110";
    public Period RecentOrdersMaximumAge { get; init; } = Period.FromDays(7);
    public Duration RemindUsersAboutDebtInterval { get; init; } = Duration.FromDays(7);
    public Period RevealLockBoxCodeBeforeReservationStart { get; init; } = Period.FromDays(3);
    public int CleaningSchedulePeriodInDays { get; init; } = 45;
    public int AdditionalDaysWhereCleaningCanBeDone { get; init; } = 3;
    public Duration RemoveAccountNumberAfter { get; init; } = Duration.FromDays(90);
    public TestingOptions? Testing { get; set; }
}
