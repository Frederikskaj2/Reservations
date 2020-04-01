namespace Frederikskaj2.Reservations.Shared
{
    public class OrderResponse<TOrder> where TOrder : class
    {
        public TOrder? Order { get; set; }
    }
}