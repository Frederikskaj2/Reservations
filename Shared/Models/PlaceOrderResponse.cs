namespace Frederikskaj2.Reservations.Shared
{
    public class PlaceOrderResponse
    {
        public PlaceOrderResult Result { get; set; }
        public MyOrder? Order { get; set; }
    }
}