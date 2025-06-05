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

sealed partial class ResidentOrderConfusion(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order1;
    State<MyOrderDto> order2;
    State<UpdateMyOrderResponse> updateMyOrderResponse1;
    State<UpdateMyOrderResponse> updateMyOrderResponse2;

    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    UpdateMyOrderResponse UpdateMyOrderResponse1 => updateMyOrderResponse1.GetValue(nameof(UpdateMyOrderResponse1));
    UpdateMyOrderResponse UpdateMyOrderResponse2 => updateMyOrderResponse2.GetValue(nameof(UpdateMyOrderResponse2));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenAnOrderIsPlacedAndPaid()
    {
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order1 = getMyOrderResponse.Order;
    }

    async Task GivenAnotherOrderIsPlaced()
    {
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Kaj.ResourceId, 1, PriceGroup.Low));
        order2 = placeMyOrderResponse.Order;
    }

    async Task WhenTheJobToConfirmOrdersExecute() =>
        await session.ConfirmOrders();

    async Task WhenTheFirstOrderIsCancelled() =>
        updateMyOrderResponse1 = await session.CancelResidentReservations(Order1.OrderId, 0);

    async Task WhenTheSecondOrderIsCancelled() =>
        updateMyOrderResponse2 = await session.CancelResidentReservations(Order2.OrderId, 0);

    async Task ThenTheFirstOrderIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order1.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        getOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheSecondOrderIsAbandoned()
    {
        var getOrderResponse = await session.GetOrder(Order2.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Abandoned);
        getOrderResponse.Order.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(OrderAuditType.PlaceOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    Task ThenTheFirstOrderHasACancellationFee()
    {
        var lineItem = UpdateMyOrderResponse1.Order.AdditionalLineItems.Single();
        lineItem.Type.Should().Be(LineItemType.CancellationFee);
        lineItem.Amount.Should().BeLessThan(Amount.Zero);
        return Task.CompletedTask;
    }

    Task ThenTheSecondOrderHasNoCancellationFee()
    {
        UpdateMyOrderResponse2.Order.AdditionalLineItems.Should().BeEmpty();
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIsThePaidPriceMinusTheCancellationFee()
    {
        var myTransactions = await session.GetMyTransactions();
        var balance = myTransactions.Transactions.Select(transaction => transaction.Amount).Sum();
        var price1 = Order1.Price.Total();
        var cancellationFee1 = UpdateMyOrderResponse1.Order.AdditionalLineItems.Single().Amount;
        balance.Should().Be(price1 + cancellationFee1);
    }
}
