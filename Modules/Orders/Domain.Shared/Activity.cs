namespace Frederikskaj2.Reservations.Orders;

public enum Activity
{
    None,
    PlaceOrder,
    UpdateOrder,
    SettleReservation,
    PayIn,
    PayOut,
    Reimburse,
    Charge, // "Opkrævning"
}
