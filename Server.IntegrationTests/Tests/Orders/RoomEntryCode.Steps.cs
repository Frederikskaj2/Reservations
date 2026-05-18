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

sealed partial class RoomEntryCode(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;

    MyOrderDto Order => order.GetValue(nameof(Order));

    Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        return Task.CompletedTask;
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

    async Task WhenTheJobToSendRoomEntryCodesExecute() =>
        await session.RunSendRoomEntryCodes();

    async Task WhenTheJobToSendRoomEntryCodesExecutesAgain()
    {
        await session.RunSendRoomEntryCodes();
        await session.DequeueEmails();
        await session.RunSendRoomEntryCodes();
    }

    async Task ThenTheUserReceivesAnEmailWithAnEntryCode()
    {
        var email = await session.DequeueRoomEntryCodeEmail();
        email.Should().NotBeNull();
        email.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        email.ToFullName.Should().Be(session.User.FullName);
        email.RoomEntryCode.Should().NotBeNull();
    }

    async Task ThenTheUserDoesNotReceiveAnEmailWithAnEntryCode()
    {
        var roomEntryCode = await session.DequeueRoomEntryCodeEmail();
        roomEntryCode.Should().BeNull();
    }
}
