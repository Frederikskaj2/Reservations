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

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Users;

sealed partial class DeleteUser(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<MyOrderDto> order;
    State<DeleteUserResponse> deleteUserResponse;
    State<UpdateMyOrderResponse> updateMyOrderResponse;

    MyOrderDto Order => order.GetValue(nameof(Order));
    DeleteUserResponse DeleteUserResponse => deleteUserResponse.GetValue(nameof(DeleteUserResponse));
    UpdateMyOrderResponse UpdateMyOrderResponse => updateMyOrderResponse.GetValue(nameof(UpdateMyOrderResponse));

    async Task IScenarioSetUp.OnScenarioSetUp() => await session.UpdateLockBoxCodes();

    async Task GivenAUserIsSignedIn() => await session.SignUpAndSignIn();

    async Task GivenTheUserHasAnUnpaidOrder()
    {
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId));
        order = placeMyOrderResponse.Order;
    }

    async Task GivenTheUserHasAPaidOrder()
    {
        var placeMyOrderResponse = await ResidentOrderExtensions.PlaceResidentOrder(session, new TestReservation(SeedData.Frederik.ResourceId));
        order = placeMyOrderResponse.Order;
        await session.PayIn(Order.Payment!.PaymentId, Order.Price.Total());
        await session.ConfirmOrders();
    }

    async Task WhenTheUserRequestsDeletion() => deleteUserResponse = await session.DeleteUser();

    async Task WhenTheOrderIsCancelledByTheUser() =>
        updateMyOrderResponse = await session.CancelResidentReservations(Order.OrderId, 0);

    async Task WhenTheOrderIsCancelledByAnAdministrator() =>
        await session.CancelReservation(Order.OrderId, 0);

    async Task WhenTheOrderIsSettledWithDamagesEqualToDeposit() =>
        await session.SettleReservation(Order.OrderId, 0, Order.Price.Deposit, "Damages");

    async Task WhenTheOrderIsSettled() =>
        await session.SettleReservation(Order.OrderId, 0);

    async Task WhenTheResidentsBalanceIsRefunded() =>
        await session.PayOut(Order.UserIdentity.UserId, Order.Price.Deposit);

    async Task WhenTheDeleteUsersJobHasExecuted() => await session.DeleteUsers();

    Task ThenTheDeletionIsSuccessful()
    {
        DeleteUserResponse.Status.Should().Be(DeleteUserStatus.Confirmed);
        return Task.CompletedTask;
    }

    Task ThenTheDeletionIsPending()
    {
        DeleteUserResponse.Status.Should().Be(DeleteUserStatus.Pending);
        return Task.CompletedTask;
    }

    Task ThenTheResponseContainsInformationAboutTheDeletion()
    {
        UpdateMyOrderResponse.IsUserDeleted.Should().BeTrue();
        return Task.CompletedTask;
    }

    async Task ThenTheUserReceivesAnEmailAboutTheDeletion()
    {
        var userDeleted = await session.DequeueUserDeletedEmail();
        userDeleted.ToEmail.Should().Be(EmailAddress.FromString(session.User!.Email));
        userDeleted.ToFullName.Should().Be(session.User.FullName);
    }

    async Task ThenTheUsersPersonalInformationIsNoLongerAvailable()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.Email.Should().Be(EmailAddress.Deleted);
        myUserResponse.Identity.FullName.Should().Be("Slettet");
        myUserResponse.Identity.Phone.Should().Be("Slettet");
        myUserResponse.Identity.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
    }

    async Task ThenTheUsersPersonalInformationIsAvailable()
    {
        var myUserResponse = await session.GetMyUser();
        myUserResponse.Identity.Email.Should().Be(EmailAddress.FromString(session.User!.Email));
        myUserResponse.Identity.FullName.Should().Be(session.User.FullName);
        myUserResponse.Identity.ApartmentId.Should().Be(session.User.ApartmentId);
    }

    async Task ThenTheUserIsPendingDeletion()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.IsDeleted.Should().BeFalse();
        getUserResponse.User.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.PlaceOrder, UserAuditType.RequestDelete);
    }

    async Task ThenTheUserIsDeleted()
    {
        var getUserResponse = await session.GetUser(session.UserId());
        getUserResponse.User.IsDeleted.Should().BeTrue();
        getUserResponse.User.Audits.Last().Type.Should().Be(UserAuditType.Delete);
    }
}
