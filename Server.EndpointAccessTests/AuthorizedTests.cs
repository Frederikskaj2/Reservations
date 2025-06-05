using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.EndpointAccessTests;

public class AuthorizedTests(ApplicationFactory factory) : IClassFixture<ApplicationFactory>
{
    [Theory]
    [ClassData(typeof(AuthorizedEndpoints))]
    public async Task Test(HttpMethod method, string path)
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(method, path);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    class AuthorizedEndpoints : TheoryData<HttpMethod, string>
    {
        public AuthorizedEndpoints()
        {
            // Bank
            Add(HttpMethod.Get, "/bank/pay-outs");
            Add(HttpMethod.Post, "/bank/pay-outs");
            Add(HttpMethod.Delete, "bank/pay-outs/0");
            Add(HttpMethod.Get, "bank/transactions");
            Add(HttpMethod.Patch, "bank/transactions/0");
            Add(HttpMethod.Post, "bank/transactions/0/reconcile/0");
            Add(HttpMethod.Post, "bank/transactions/0/reconcile-pay-out/0");
            Add(HttpMethod.Post, "bank/transactions/import");
            Add(HttpMethod.Get, "bank/transactions/range");
            Add(HttpMethod.Get, "postings");
            Add(HttpMethod.Get, "postings/range");
            Add(HttpMethod.Post, "postings/send");
            // Calendar
            Add(HttpMethod.Get, "reserved-days/my");
            Add(HttpMethod.Get, "reserved-days/owner");
            // Cleaning
            Add(HttpMethod.Get, "cleaning-schedule");
            Add(HttpMethod.Post, "cleaning-schedule/send");
            // LockBox
            Add(HttpMethod.Get, "lock-box-codes");
            Add(HttpMethod.Post, "lock-box-codes/send");
            // Orders
            Add(HttpMethod.Get, "creditors");
            Add(HttpMethod.Get, "orders");
            Add(HttpMethod.Get, "orders/0");
            Add(HttpMethod.Get, "orders/my");
            Add(HttpMethod.Post, "orders/my");
            Add(HttpMethod.Get, "orders/my/0");
            Add(HttpMethod.Patch, "orders/my/0");
            Add(HttpMethod.Post, "orders/owner");
            Add(HttpMethod.Patch, "orders/owner/0");
            Add(HttpMethod.Post, "orders/resident");
            Add(HttpMethod.Patch, "orders/resident/0");
            Add(HttpMethod.Patch, "orders/resident/0/reservations");
            Add(HttpMethod.Post, "orders/resident/0/settle-reservation");
            Add(HttpMethod.Get, "reports/yearly-summary");
            Add(HttpMethod.Get, "reports/yearly-summary/range");
            Add(HttpMethod.Get, "residents");
            Add(HttpMethod.Get, "transactions");
            Add(HttpMethod.Get, "users/0/transactions");
            Add(HttpMethod.Post, "users/0/reimburse");
            // Users
            Add(HttpMethod.Get, "user");
            Add(HttpMethod.Patch, "user");
            Add(HttpMethod.Delete, "user");
            Add(HttpMethod.Post, "user/resend-confirm-email-email");
            Add(HttpMethod.Post, "user/sign-out-everywhere-else");
            Add(HttpMethod.Post, "user/update-password");
            Add(HttpMethod.Get, "users");
            Add(HttpMethod.Get, "users/0");
            Add(HttpMethod.Patch, "users/0");
        }
    }
}
