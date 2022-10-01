namespace Frederikskaj2.Reservations.Shared.Core;

public enum OrderAuditType
{
    None,
    Import,
    PlaceOrder,
    ConfirmOrder,
    SettleReservation,
    CancelReservation,
    AllowCancellationWithoutFee,
    DisallowCancellationWithoutFee,
    UpdateDescription,
    UpdateCleaning,
    FinishOrder
}
