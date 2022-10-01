using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class EarlyOrder : IClassFixture<SessionFixture>
{
    public EarlyOrder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task PlaceAndPayEarlyOrderCannotBeCancelledByUser()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, true));
        var myOrder = await Session.GetMyOrderAsync(userOrder.OrderId);
        var reservation = myOrder.Reservations!.First();
        var response = await Session.CancelUserReservationRawAsync(userOrder.OrderId, 0);
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.CanBeCancelled.Should().BeFalse();
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task PlaceAndPayEarlyThenCancelByAdministrator()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, true));
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        await Session.CancelReservationAsync(Session.UserId(), order.OrderId, 0);
        var cancelledOrder = await Session.GetOrderAsync(userOrder.OrderId);
        var reservation = cancelledOrder.Reservations.First();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task PlaceAndPayEarlyOrderCanBeCancelledByUserIfAllowed()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 1, true));
        var order = await Session.GetOrderAsync(userOrder.OrderId);
        await Session.AllowUserToCancelWithoutFee(Session.UserId(), order.OrderId);
        var myOrder = await Session.GetMyOrderAsync(userOrder.OrderId);
        var reservation = myOrder.Reservations!.First();
        var result = await Session.UserCancelReservationsAsync(userOrder.OrderId, 0);
        reservation.CanBeCancelled.Should().BeTrue();
        var cancelledReservation = result.Order!.Reservations!.First();
        cancelledReservation.Status.Should().Be(ReservationStatus.Cancelled);
    }
}
