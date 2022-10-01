using FluentAssertions;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class DeleteUser : IClassFixture<SessionFixture>
{
    public DeleteUser(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task SignUpThenDelete()
    {
        await Session.SignUpAndSignInAsync();
        var response = await Session.DeleteUserAsync();
        var userDeleted = await Session.DequeueUserDeletedEmailAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.Success);
        userDeleted.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        userDeleted.FullName.Should().Be(Session.User.FullName);
        myUser.Information.Email.Should().Be(EmailAddress.Deleted);
        myUser.Information.FullName.Should().Be("Slettet");
        myUser.Information.Phone.Should().Be("Slettet");
        myUser.Information.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
        user.IsDeleted.Should().BeTrue();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.RequestDelete, UserAuditType.Delete);
    }

    [Fact]
    public async Task CannotDeleteWithActiveOrder()
    {
        await Session.SignUpAndSignInAsync();
        await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var response = await Session.DeleteUserAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.IsPendingDelete);
        myUser.Information.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        myUser.Information.FullName.Should().Be(Session.User.FullName);
        myUser.Information.ApartmentId.Should().Be(Session.User.ApartmentId);
        user.IsDeleted.Should().BeFalse();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.RequestDelete);
    }

    [Fact]
    public async Task CompleteDeletionAfterOrderCancellation()
    {
        await Session.SignUpAndSignInAsync();
        var order = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var response = await Session.DeleteUserAsync();
        await Session.UserCancelReservationsAsync(order.OrderId, 0);
        var userDeleted = await Session.DequeueUserDeletedEmailAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.IsPendingDelete);
        userDeleted.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        userDeleted.FullName.Should().Be(Session.User.FullName);
        myUser.Information.Email.Should().Be(EmailAddress.Deleted);
        myUser.Information.FullName.Should().Be("Slettet");
        myUser.Information.Phone.Should().Be("Slettet");
        myUser.Information.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
        user.IsDeleted.Should().BeTrue();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.RequestDelete,
            UserAuditType.RemoveAccountNumber, UserAuditType.Delete);
    }

    [Fact]
    public async Task CompleteDeletionAfterOrderCancellationByAdministrator()
    {
        await Session.SignUpAndSignInAsync();
        var order = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var response = await Session.DeleteUserAsync();
        await Session.CancelReservationAsync(Session.UserId(), order.OrderId, 0);
        var userDeleted = await Session.DequeueUserDeletedEmailAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.IsPendingDelete);
        userDeleted.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        userDeleted.FullName.Should().Be(Session.User.FullName);
        myUser.Information.Email.Should().Be(EmailAddress.Deleted);
        myUser.Information.FullName.Should().Be("Slettet");
        myUser.Information.Phone.Should().Be("Slettet");
        myUser.Information.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
        user.IsDeleted.Should().BeTrue();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.RequestDelete,
            UserAuditType.RemoveAccountNumber, UserAuditType.Delete);
    }

    [Fact]
    public async Task CompleteDeletionAfterOrderSettlement()
    {
        await Session.SignUpAndSignInAsync();
        var order = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var response = await Session.DeleteUserAsync();
        await Session.SettleReservationAsync(Session.UserId(), order.OrderId, 0, order.Price.Deposit, "Test");
        var userDeleted = await Session.DequeueUserDeletedEmailAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.IsPendingDelete);
        userDeleted.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        userDeleted.FullName.Should().Be(Session.User.FullName);
        myUser.Information.Email.Should().Be(EmailAddress.Deleted);
        myUser.Information.FullName.Should().Be("Slettet");
        myUser.Information.Phone.Should().Be("Slettet");
        myUser.Information.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
        user.IsDeleted.Should().BeTrue();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.PayIn,
            UserAuditType.RequestDelete, UserAuditType.RemoveAccountNumber, UserAuditType.Delete);
    }

    [Fact]
    public async Task CompleteDeletionAfterPayout()
    {
        await Session.SignUpAndSignInAsync();
        var order = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        var response = await Session.DeleteUserAsync();
        await Session.SettleReservationAsync(Session.UserId(), order.OrderId, 0);
        await Session.PayOutAsync(Session.UserId(), order.Price.Deposit);
        var userDeleted = await Session.DequeueUserDeletedEmailAsync();
        var myUser = await Session.GetMyUserAsync();
        var user = await Session.GetUserAsync(Session.UserId());
        response.Result.Should().Be(DeleteUserResult.IsPendingDelete);
        userDeleted.Email.Should().Be(EmailAddress.FromString(Session.User!.Email));
        userDeleted.FullName.Should().Be(Session.User.FullName);
        myUser.Information.Email.Should().Be(EmailAddress.Deleted);
        myUser.Information.FullName.Should().Be("Slettet");
        myUser.Information.Phone.Should().Be("Slettet");
        myUser.Information.ApartmentId.Should().Be(Apartment.Deleted.ApartmentId);
        user.IsDeleted.Should().BeTrue();
        user.Audits.Select(audit => audit.Type).Should().Equal(
            UserAuditType.SignUp, UserAuditType.ConfirmEmail, UserAuditType.SetAccountNumber, UserAuditType.CreateOrder, UserAuditType.PayIn,
            UserAuditType.RequestDelete, UserAuditType.PayOut, UserAuditType.RemoveAccountNumber, UserAuditType.Delete);
    }
}
