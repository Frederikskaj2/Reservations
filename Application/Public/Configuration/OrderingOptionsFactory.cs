using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Options;

namespace Frederikskaj2.Reservations.Application;

public class OrderingOptionsFactory : IOptionsFactory<OrderingOptions>
{
    readonly ApplicationOptions applicationOptions;
    readonly TestingOptions testingOptions;

    public OrderingOptionsFactory(IOptionsSnapshot<ApplicationOptions> applicationOptions, IOptionsSnapshot<TestingOptions> testingOptions)
    {
        this.applicationOptions = applicationOptions.Value;
        this.testingOptions = testingOptions.Value;
    }

    public OrderingOptions Create(string name) => new()
    {
        AdditionalDaysWhereCleaningCanBeDone = applicationOptions.AdditionalDaysWhereCleaningCanBeDone,
        CancellationFee = applicationOptions.CancellationFee,
        CheckInTime = applicationOptions.CheckInTime,
        CheckOutTime = applicationOptions.CheckOutTime,
        CleaningSchedulePeriodInDays = applicationOptions.CleaningSchedulePeriodInDays,
        MaximumAllowedReservationNights = applicationOptions.MaximumAllowedReservationNights,
        MinimumCancellationNoticeInDays = applicationOptions.MinimumCancellationNoticeInDays,
        CancellationWithoutFeeDuration = applicationOptions.CancellationWithoutFeeDuration,
        PayInAccountNumber = applicationOptions.PayInAccountNumber,
        RemindUsersAboutDebtInterval = applicationOptions.RemindUsersAboutDebtInterval,
        RevealLockBoxCodeDaysBeforeReservationStart = applicationOptions.RevealLockBoxCodeDaysBeforeReservationStart,
        PaymentDeadlineInDays = applicationOptions.PaymentDeadlineInDays,
        Prices = applicationOptions.Prices,
        ReservationIsNotAllowedAfterDaysFromNow = applicationOptions.ReservationIsNotAllowedAfterDaysFromNow,
        ReservationIsNotAllowedBeforeDaysFromNow = applicationOptions.ReservationIsNotAllowedBeforeDaysFromNow,
        Testing = testingOptions.IsTestingEnabled ? testingOptions : null
    };
}
