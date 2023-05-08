using FluentAssertions;
using Frederikskaj2.Reservations.Application.BackgroundServices;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests;

public class DebtReminder : IClassFixture<SessionFixture>
{
    public DebtReminder(SessionFixture session) => Session = session;

    SessionFixture Session { get; }

    [Fact]
    public async Task UnpaidOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId));
        Session.NowOffset = Period.FromDays(8);
        using var serviceScope = Session.CreateServiceScope();
        var debtReminderService = serviceScope.ServiceProvider.GetRequiredService<DebtReminderService>();
        await debtReminderService.SendDebtReminders(Session.CurrentDate.PlusDays(8).At(LocalTime.Midnight).InZoneLeniently(DateTimeZone.Utc).ToInstant());
        var emails = await Session.DequeueEmailsAsync();
        var debtRemindEmail = emails.DebtReminder();
        debtRemindEmail.Should().NotBeNull();
        userOrder.Payment!.Amount.Should().Be(debtRemindEmail!.Payment!.Amount);
    }

    [Fact]
    public async Task ReplacePaidOrderWithSimilarOrder()
    {
        await Session.SignUpAndSignInAsync();
        var userOrder1 = await Session.UserPlaceAndPayOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        var userOrder2 = await Session.UserPlaceOrderAsync(new TestReservation(TestData.Frederik.ResourceId, 1, PriceGroup.Low));
        await Session.AllowUserToCancelWithoutFee(Session.UserId(), userOrder1.OrderId);
        await Session.CancelUserReservationNoFeeAsync(userOrder1.OrderId, 0);
        var order2 = await Session.GetOrderAsync(userOrder2.OrderId);
        var myOrder2 = await Session.GetMyOrderAsync(userOrder2.OrderId);
        Session.NowOffset = Period.FromDays(8);
        using var serviceScope = Session.CreateServiceScope();
        var debtReminderService = serviceScope.ServiceProvider.GetRequiredService<DebtReminderService>();
        await debtReminderService.SendDebtReminders(Session.CurrentDate.PlusDays(8).At(LocalTime.Midnight).InZoneLeniently(DateTimeZone.Utc).ToInstant());
        var emails = await Session.DequeueEmailsAsync();
        var debtRemindEmail = emails.DebtReminder();
        order2.IsHistoryOrder.Should().BeFalse();
        order2.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
        myOrder2.Payment.Should().BeNull();
        debtRemindEmail.Should().BeNull();
    }
}
