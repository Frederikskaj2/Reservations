using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class LockBoxCodes : IClassFixture<SessionFixture>
{
    const int revealLockBoxCodeDaysBeforeReservationStart = 3;

    public LockBoxCodes(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task CodesAreHidden()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var reservation = userOrder.Reservations!.Single();
        reservation.LockBoxCodes.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CodesAreRevealed()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var period = userOrder.Reservations!.Single().Extent.Date - Session.CurrentDate + Period.FromDays(-revealLockBoxCodeDaysBeforeReservationStart);
        Session.NowOffset += period;
        var order = await Session.GetMyOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations!.Single();
        reservation.LockBoxCodes.Should().NotBeEmpty();
        reservation.LockBoxCodes.Should().HaveCount(1);
    }

    [Fact]
    public async Task TwoCodesWithReservationFromSundayToMonday()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Kaj.ResourceId, 7, PriceGroup.High));
        var period = userOrder.Reservations!.Single().Extent.Date - Session.CurrentDate + Period.FromDays(-revealLockBoxCodeDaysBeforeReservationStart);
        Session.NowOffset += period;
        var order = await Session.GetMyOrderAsync(userOrder.OrderId);
        var reservation = order.Reservations!.Single();
        reservation.LockBoxCodes.Should().NotBeEmpty();
        reservation.LockBoxCodes.Should().HaveCount(2);
    }
}
