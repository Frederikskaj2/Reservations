using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Orders;

static class GetMyOrder
{
    public static Either<Failure<Unit>, GetMyOrderOutput> GetMyOrderCore(OrderingOptions options, GetMyOrderInput input) =>
        input.Order.UserId == input.Query.UserId
            ? new GetMyOrderOutput(input.Order, input.User)
            : Failure.New(HttpStatusCode.NotFound, $"Order {input.Order.OrderId} does not belong to user {input.Query.UserId}.");
}
