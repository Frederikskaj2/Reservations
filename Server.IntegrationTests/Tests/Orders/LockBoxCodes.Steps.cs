using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class LockBoxCodes(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;

    MyOrderDto Order => order.GetValue(nameof(Order));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        await session.UpdateLockBoxCodes();
    }

    async Task GivenAResidentHasPlacedAndPaidAnOrder(int numberOfReservations)
    {
        await session.SignUpAndSignIn();
        var reservations = Enumerable.Repeat(new TestReservation(SeedData.Kaj.ResourceId, 4, TestReservationType.Soon), numberOfReservations).ToArray();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(reservations);
        order = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    Task GivenTheFirstReservationWillStartInAFewDays()
    {
        session.NowOffset = Period.Between(session.CurrentDate, Order.Reservations.First().Extent.Date.PlusDays(-1), PeriodUnits.Days);
        return Task.CompletedTask;
    }

    async Task WhenTheJobToSendLockBoxCodesExecute() =>
        await session.RunSendLockBoxCodes();

    async Task WhenTheJobToSendLockBoxCodesExecutesAgain()
    {
        await session.RunSendLockBoxCodes();
        await session.DequeueEmails();
        await session.RunSendLockBoxCodes();
    }

    async Task ThenTheUserReceivesAnEmailWithLockBoxCodes()
    {
        var lockBoxCodes = await session.DequeueLockBoxCodesEmail();
        lockBoxCodes.Should().NotBeNull();
        lockBoxCodes.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        lockBoxCodes.ToFullName.Should().Be(session.User.FullName);
        lockBoxCodes.LockBoxCodes.Should().NotBeNull();
        var reservation = Order.Reservations.First();
        lockBoxCodes.LockBoxCodes.Date.Should().Be(reservation.Extent.Date);
        lockBoxCodes.LockBoxCodes.DatedLockBoxCodes.Should().NotBeEmpty();
        lockBoxCodes.LockBoxCodes.OrderId.Should().Be(Order.OrderId);
        lockBoxCodes.LockBoxCodes.ResourceId.Should().Be(reservation.ResourceId);
    }

    async Task ThenTheUserDoesNotReceiveAnEmailWithLockBoxCodes()
    {
        var lockBoxCodes = await session.DequeueLockBoxCodesEmail();
        lockBoxCodes.Should().BeNull();
    }
}
