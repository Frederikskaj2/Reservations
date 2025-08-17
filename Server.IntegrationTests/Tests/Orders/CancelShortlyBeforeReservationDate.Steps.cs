using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class CancelShortlyBeforeReservationDate(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<HttpResponseMessage> response;

    MyOrderDto Order => order.GetValue(nameof(Order));
    HttpResponseMessage Response => response.GetValue(nameof(Response));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        await session.UpdateLockBoxCodes();
    }

    async Task GivenAConfirmedOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, Type: TestReservationType.Soon));
        order = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    Task GivenTheDeadlineForResidentCancellationIsInThePast()
    {
        session.NowOffset += Period.FromDays(2);
        return Task.CompletedTask;
    }

    async Task GivenTheResidentIsAllowedToCancelWithoutFee() =>
        await session.AllowResidentToCancelWithoutFee(Order.OrderId);

    async Task WhenTheResidentCancelsTheReservation() =>
        response = await session.UpdateResidentReservationRaw(Order.OrderId, 0);

    async Task WhenTheAdministratorCancelsTheReservation() =>
        response = await session.CancelReservationRaw(Order.OrderId, 0);

    async Task ThenTheReservationCannotBeCancelledByTheResident()
    {
        var getMyOrderResponse = await session.GetMyOrder(Order.OrderId);
        var reservation = getMyOrderResponse.Order.Reservations.Single();
        reservation.CanBeCancelled.Should().BeFalse();
    }

    Task ThenTheRequestToCancelTheReservationFails()
    {
        Response.IsSuccessStatusCode.Should().BeFalse();
        return Task.CompletedTask;
    }

    Task ThenTheRequestToCancelTheReservationIsSuccessful()
    {
        Response.IsSuccessStatusCode.Should().BeTrue();
        return Task.CompletedTask;
    }

    async Task ThenTheReservationIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        var reservation = getOrderResponse.Order.Reservations.Single();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }
}
