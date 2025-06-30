using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class ResidentOrderPlacedTooLate(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<HttpResponseMessage> response;

    MyOrderDto Order => order.GetValue(nameof(Order));
    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResident() => await session.SignUpAndSignIn();

    async Task WhenAnAdministratorPlacesALateOrderForTheResident()
    {
        var getMyOrderResponse = await session.AdministratorPlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, Type: TestReservationType.Tomorrow));
        order = getMyOrderResponse.Order;
    }

    async Task WhenTheResidentPlacesALateOrder() =>
        response = await session.PlaceResidentOrderRaw(new TestReservation(SeedData.Frederik.ResourceId, Type: TestReservationType.Tomorrow));

    async Task ThenTheOrderIsPlaced()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Reserved);
        getOrderResponse.Order.Audits.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new
            {
                UserId = SeedData.AdministratorUserId,
                Type = OrderAuditType.PlaceOrder,
            });
    }

    async Task ThenTheOrderPlacementIsAuditedForTheAdministrator()
    {
        var getUserResponse = await session.GetUser(SeedData.AdministratorUserId);
        getUserResponse.User.Audits.Should().ContainEquivalentOf(
            new
            {
                UserId = SeedData.AdministratorUserId,
                Type = UserAuditType.PlaceOrder,
                Order.OrderId,
            });
    }

    Task ThenTheRequestIsDenied()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        return Task.CompletedTask;
    }
}
