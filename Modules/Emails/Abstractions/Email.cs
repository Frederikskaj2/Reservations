using Frederikskaj2.Reservations.Users;
using System;
using System.Linq;
using System.Reflection;

namespace Frederikskaj2.Reservations.Emails;

public record Email(EmailAddress ToEmail, string ToFullName, Uri FromUrl)
{
    public CleaningScheduleOverviewDto? CleaningScheduleOverview { get; init; }
    public ConfirmEmailDto? ConfirmEmail { get; init; }
    public DebtReminderDto? DebtReminder { get; init; }
    public LockBoxCodesDto? LockBoxCodes { get; init; }
    public LockBoxCodesOverviewDto? LockBoxCodesOverview { get; init; }
    public NewOrderDto? NewOrder { get; init; }
    public NewPasswordDto? NewPassword { get; init; }
    public NoFeeCancellationAllowedDto? NoFeeCancellationAllowed { get; init; }
    public OrderConfirmedDto? OrderConfirmed { get; init; }
    public OrderReceivedDto? OrderReceived { get; init; }
    public PayInDto? PayIn { get; init; }
    public PayOutDto? PayOut { get; init; }
    public PostingsForMonthDto? PostingsForMonth { get; init; }
    public ReservationsCancelledDto? ReservationsCancelled { get; init; }
    public ReservationSettledDto? ReservationSettled { get; init; }
    public SettlementNeededDto? SettlementNeeded { get; init; }
    public UserDeletedDto? UserDeleted { get; init; }

    public override string ToString() =>
        GetNonNullProperty()?.Name ?? "<empty>";

    PropertyInfo? GetNonNullProperty() =>
        GetType()
            .GetProperties()
            .FirstOrDefault(
                property =>
                    property.PropertyType != typeof(EmailAddress) &&
                    property.PropertyType != typeof(string) &&
                    property.PropertyType != typeof(Uri) &&
                    property.GetValue(this) is not null);
}
