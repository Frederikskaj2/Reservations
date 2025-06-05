using Frederikskaj2.Reservations.Orders;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class ResidentExtensions
{
    public static async ValueTask<GetMyOrdersResponse> GetMyOrders(this SessionFixture session) =>
        await session.Deserialize<GetMyOrdersResponse>(await session.Get("orders/my"));

    public static async ValueTask<GetMyTransactionsResponse> GetMyTransactions(this SessionFixture session) =>
        await session.Deserialize<GetMyTransactionsResponse>(await session.Get("transactions"));
}
