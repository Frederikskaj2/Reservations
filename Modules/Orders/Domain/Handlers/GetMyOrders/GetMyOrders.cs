namespace Frederikskaj2.Reservations.Orders;

static class GetMyOrders
{
    public static GetMyOrdersOutput GetMyOrdersCore(OrderingOptions options, GetMyOrdersInput input) =>
        new(input.User, input.Orders, OrdersLockBoxCodesFunctions.CreateLockBoxCodesForOrders(options, input.Query.Today, input.Orders, input.LockBoxCodes));
}