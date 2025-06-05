namespace Frederikskaj2.Reservations.Orders;

public enum TransactionDescriptionType
{
    None,
    Cancellation,
    Settlement,
    ReservationsUpdate,
    Reimbursement,
    Charge, // "Opkrævning"
}
