using System;
using System.Linq;

namespace Frederikskaj2.Reservations.Shared.Email;

public record Email(Uri FromUrl)
{
    public CleaningSchedule? CleaningSchedule { get; init; }
    public ConfirmEmail? ConfirmEmail { get; init; }
    public DebtReminder? DebtReminder { get; init; }
    public LockBoxCodes? LockBoxCodes { get; init; }
    public LockBoxCodesOverview? LockBoxCodesOverview { get; init; }
    public NewOrder? NewOrder { get; init; }
    public NewPassword? NewPassword { get; init; }
    public NoFeeCancellationAllowed? NoFeeCancellationAllowed { get; init; }
    public OrderConfirmed? OrderConfirmed { get; init; }
    public OrderReceived? OrderReceived { get; init; }
    public PayIn? PayIn { get; init; }
    public PayOut? PayOut { get; init; }
    public PostingsForMonth? PostingsForMonth { get; init; }
    public ReservationsCancelled? ReservationsCancelled { get; init; }
    public ReservationSettled? ReservationSettled { get; init; }
    public SettlementNeeded? SettlementNeeded { get; init; }
    public UserDeleted? UserDeleted { get; init; }

    public override string ToString() =>
        GetType()
            .GetProperties()
            .FirstOrDefault(property => property.PropertyType != typeof(Uri) && property.GetValue(this) is not null)?
            .PropertyType.Name ?? "<empty>";
}
