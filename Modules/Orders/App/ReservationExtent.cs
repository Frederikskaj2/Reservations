namespace Frederikskaj2.Reservations.Orders;

static class ReservationExtent
{
    public static ReservationExtent<T> Create<T>(T key, Extent extent) => new(key, extent);
}

record ReservationExtent<T>(T Key, Extent Extent);
