namespace Frederikskaj2.Reservations.Shared
{
    public class PlaceOwnerOrderResponse
    {
        public PlaceOrderResult Result { get; set; }
        public OwnerOrder? Order { get; set; }
    }
}