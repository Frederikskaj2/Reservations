using NodaTime;
using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared.Core;
using System;

namespace Frederikskaj2.Reservations.Application;

public class ApplicationOptions
{
    public IReadOnlyDictionary<ResourceType, PriceOptions> Prices { get; } = new Dictionary<ResourceType, PriceOptions>
    {
        {
            ResourceType.BanquetFacilities, new PriceOptions
            {
                LowRentPerNight = 500,
                HighRentPerNight = 750,
                Cleaning = 500,
                LowDeposit = 1000,
                HighDeposit = 3000
            }
        },
        {
            ResourceType.Bedroom, new PriceOptions
            {
                LowRentPerNight = 200,
                HighRentPerNight = 250,
                Cleaning = 200,
                LowDeposit = 500,
                HighDeposit = 500
            }
        }
    };

    public int ReservationIsNotAllowedBeforeDaysFromNow { get; init; } = 5;
    public int ReservationIsNotAllowedAfterDaysFromNow { get; init; } = 274;
    // The key code logic expects that at most two key codes are required for a single reservation.
    public int MaximumAllowedReservationNights { get; init; } = 7;
    public LocalTime CheckInTime { get; init; } = new(12, 0);
    public LocalTime CheckOutTime { get; init; } = new(10, 0);
    public Amount CancellationFee { get; init; } = 200;
    public int MinimumCancellationNoticeInDays { get; init; } = 14;
    public Duration CancellationWithoutFeeDuration { get; init; } = Duration.FromDays(5);
    public int PaymentDeadlineInDays { get; init; } = 7;
    public string PayInAccountNumber { get; init; } = "9444-12501110";
    public Duration RemindUsersAboutDebtInterval { get; init; } = Duration.FromDays(7);
    public int RevealLockBoxCodeDaysBeforeReservationStart { get; init; } = 3;
    public int CleaningSchedulePeriodInDays { get; init; } = 45;
    public int AdditionalDaysWhereCleaningCanBeDone { get; init; } = 3;
    public Uri BaseUrl { get; init; } = new("https://lokaler.frederikskaj2.dk/");
}
