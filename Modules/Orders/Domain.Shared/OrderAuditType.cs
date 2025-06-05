namespace Frederikskaj2.Reservations.Orders;

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
    FinishOrder,
    UpdateReservations,
}
