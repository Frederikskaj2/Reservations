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

sealed partial class PaymentTransferTwoOrders(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<ResidentDto> resident;
    State<MyOrderDto> order1;
    State<MyOrderDto> order2;
    State<UpdateMyOrderResponse> updateMyOrderResponse;

    ResidentDto Resident => resident.GetValue(nameof(Resident));
    MyOrderDto Order1 => order1.GetValue(nameof(Order1));
    MyOrderDto Order2 => order2.GetValue(nameof(Order2));
    UpdateMyOrderResponse UpdateMyOrderResponse => updateMyOrderResponse.GetValue(nameof(UpdateMyOrderResponse));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAndPaidAnOrder()
    {
        await session.SignUpAndSignIn();
        var getMyOrderResponse = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order1 = getMyOrderResponse.Order;
        await session.RunConfirmOrders();
    }

    async Task GivenTheResidentHasCancelledTheOrder() =>
        updateMyOrderResponse = await session.CancelResidentReservations(Order1.OrderId, 0);

    async Task WhenTheResidentPlacesASimilarOrderAndPaysTheOutstandingAmount()
    {
        var placeMyOrderResponse = await session.PlaceResidentOrder(new TestReservation(SeedData.Frederik.ResourceId, 1, PriceGroup.Low));
        order2 = placeMyOrderResponse.Order;
        resident = await session.GetMyResident();
        await session.PayIn(Resident.PaymentId, -Resident.Balance);
        await session.RunConfirmOrders();
    }

    async Task ThenTheFirstOrderIsCancelled()
    {
        var getOrderResponse = await session.GetOrder(Order1.OrderId);
        getOrderResponse.Order.Should().NotBeNull();
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeTrue();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(
            OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder, OrderAuditType.CancelReservation, OrderAuditType.FinishOrder);
    }

    async Task ThenTheSecondOrderIsConfirmed()
    {
        var getOrderResponse = await session.GetOrder(Order2.OrderId);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder, OrderAuditType.ConfirmOrder);
    }

    Task ThenTheAmountPaidTheSecondTimeIsTheCancellationFee()
    {
        var residualPayment = Resident.Balance;
        var cancellationFee = UpdateMyOrderResponse.Order.AdditionalLineItems.Single().Amount;
        residualPayment.Should().Be(cancellationFee);
        return Task.CompletedTask;
    }

    async Task ThenTheResidentsBalanceIs0()
    {
        var getMyTransactionsResponse = await session.GetMyTransactions();
        var balance = getMyTransactionsResponse.Transactions.Select(transaction => transaction.Amount).Sum();
        balance.Should().Be(Amount.Zero);
    }
}
