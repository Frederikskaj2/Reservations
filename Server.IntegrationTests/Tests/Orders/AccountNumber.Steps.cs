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

sealed partial class AccountNumber(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    const string newAccountNumber = "9876-123456789";

    State<MyOrderDto> order;

    MyOrderDto Order => order.GetValue(nameof(Order));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAResidentHasPlacedAnOrder()
    {
        await session.SignUpAndSignIn();
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Kaj.ResourceId));
        order = placeMyOrderResponse.Order;
    }

    async Task WhenTheResidentUpdatesTheAccountNumber() => await session.UpdateResidentAccountNumber(Order.OrderId, newAccountNumber);

    async Task WhenTheAdministratorUpdatesTheAccountNumber() =>
        await session.UpdateAccountNumber(Order.OrderId, newAccountNumber);

    async Task ThenTheOrderHasTheNewAccountNumber()
    {
        var getOrderResponse = await session.GetOrder(Order.OrderId);
        getOrderResponse.Order.Should().NotBeNull();
        getOrderResponse.Order.Type.Should().Be(OrderType.Resident);
        getOrderResponse.Order.IsHistoryOrder.Should().BeFalse();
        getOrderResponse.Order.Resident!.AccountNumber.Should().Be(newAccountNumber);
        getOrderResponse.Order.Audits.Select(audit => audit.Type).Should().Equal(OrderAuditType.PlaceOrder);
    }

    async Task ThenTheResidentCanSeeTheUpdatedBankAccountNumber()
    {
        var myUser = await session.GetMyUser();
        myUser.AccountNumber.Should().Be(newAccountNumber);
    }

    async Task ThenTheResidentsChangeOfAccountNumberIsAudited()
    {
        var getUserResponse = await session.GetUser(Order.UserIdentity.UserId);
        getUserResponse.User.Should().NotBeNull();
        getUserResponse.User.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.PlaceOrder, UserAuditType.SetAccountNumber);
    }

    async Task ThenTheAdministratorsChangeOfAccountNumberIsAudited()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.Audits.Select(audit => audit.Type)
            .Should()
            .Equal(UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.PlaceOrder, UserAuditType.SetAccountNumber);
        getUserResponse.User.Audits.ElementAt(2).Should().Match<UserAuditDto>(audit => audit.UserId == session.UserId());
        getUserResponse.User.Audits.ElementAt(4).Should().Match<UserAuditDto>(audit => audit.UserId == SeedData.AdministratorUserId);
    }
}
