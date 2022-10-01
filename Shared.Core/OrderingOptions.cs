using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public class OrderingOptions
{
    static readonly Dictionary<ResourceType, PriceOptions> empty = new();

    public IReadOnlyDictionary<ResourceType, PriceOptions> Prices { get; init; } = empty;
    public int ReservationIsNotAllowedBeforeDaysFromNow { get; init; }
    public int ReservationIsNotAllowedAfterDaysFromNow { get; init; }
    public int MaximumAllowedReservationNights { get; init; }
    public LocalTime CheckInTime { get; init; }
    public LocalTime CheckOutTime { get; init; }
    public Amount CancellationFee { get; init; }
    public int MinimumCancellationNoticeInDays { get; init; }
    public int PaymentDeadlineInDays { get; init; }
    public Duration CancellationWithoutFeeDuration { get; init; }
    public string PayInAccountNumber { get; init; } = "";
    public Duration RemindUsersAboutDebtInterval { get; init; }
    public int RevealLockBoxCodeDaysBeforeReservationStart { get; init; }
    public int CleaningSchedulePeriodInDays { get; init; }
    public int AdditionalDaysWhereCleaningCanBeDone { get; init; }
    public TestingOptions? Testing { get; init; }
}
