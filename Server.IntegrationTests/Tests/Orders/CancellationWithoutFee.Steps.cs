using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class CancellationWithoutFee(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;

    MyOrderDto Order => order.GetValue(nameof(Order));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAndPaidAnOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Kaj.ResourceId, 1, TestReservationType.Soon));
        order = getMyOrderResponse.Order;
        await session.ConfirmOrders();
    }

    async Task AndGivenAnAdministratorHasAllowedTheResidentToCancelWithoutAFee() =>
        await session.AllowResidentToCancelWithoutFee(Order.OrderId);

    async Task WhenTheResidentCancelsTheOrder()
    {
        var updateMyOrderResponse = await session.CancelResidentReservationNoFee(Order.OrderId, 0);
        order = updateMyOrderResponse.Order;
    }

    Task ThenTheOrderIsCancelled()
    {
        Order.IsHistoryOrder.Should().BeTrue();
        Order.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        return Task.CompletedTask;
    }

    async Task AndTheResidentDoesNotHaveAnyReservedDaysInTheCalendar()
    {
        var getReservedDaysResponse = await session.GetMyReservedDays();
        var myReservedDays = getReservedDaysResponse.ReservedDays.Where(reservedDay => reservedDay.IsMyReservation);
        myReservedDays.Should().BeEmpty();
    }

    async Task AndTheFullPriceOfTheOrderIsOwedToTheResident()
    {
        var myTransactions = await session.GetMyTransactions();
        var myBalance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        myBalance.Should().Be(Order.Price.Total());
    }
}
