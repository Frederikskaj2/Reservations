using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.EndpointAccessTests;

public class AnonymousTests(ApplicationFactory factory) : IClassFixture<ApplicationFactory>
{
    [Theory]
    [ClassData(typeof(AnonymousEndpoints))]
    public async Task Test(HttpMethod method, string path, HttpStatusCode expectedStatusCode)
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(method, path);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    class AnonymousEndpoints : TheoryData<HttpMethod, string, HttpStatusCode>
    {
        public AnonymousEndpoints()
        {
            // Calendar
            // Core
            Add(HttpMethod.Get, "configuration", HttpStatusCode.OK);
            // Users
            Add(HttpMethod.Post, "user/confirm-email", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/create-access-token", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/new-password", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/send-new-password-email", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/sign-in", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/sign-out", HttpStatusCode.BadRequest);
            Add(HttpMethod.Post, "user/sign-up", HttpStatusCode.BadRequest);
        }
    }
}
