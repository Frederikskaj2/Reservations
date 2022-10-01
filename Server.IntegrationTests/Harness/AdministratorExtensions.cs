using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class AdministratorExtensions
{
    public static async ValueTask<UserDetails> GetUserAsync(this SessionFixture session, UserId userId) =>
        await session.DeserializeAsync<UserDetails>(await session.AdministratorGetAsync($"users/{userId}"));

    public static async ValueTask<IEnumerable<User>> GetUsersAsync(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<User>>(await session.AdministratorGetAsync($"users"));

    public static async ValueTask<IEnumerable<Order>> GetUserOrdersAsync(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<Order>>(await session.AdministratorGetAsync("orders/user"));

    public static async ValueTask<IEnumerable<Order>> GetOwnerOrdersAsync(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<Order>>(await session.AdministratorGetAsync("orders/owner"));

    public static async ValueTask<OrderDetails> GetOrderAsync(this SessionFixture session, OrderId orderId) =>
        await session.DeserializeAsync<OrderDetails>(await session.AdministratorGetAsync($"orders/any/{orderId}"));

    public static async ValueTask<IEnumerable<Debtor>> GetDebtorsAsync(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<Debtor>>(await session.AdministratorGetAsync("debtors"));

    public static async ValueTask<Debtor> GetUserDebtorAsync(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var debtors = await session.GetDebtorsAsync();
        return debtors.Single(d => d.UserInformation.Email == session.User.Email);
    }

    public static async ValueTask<IEnumerable<Creditor>> GetCreditorsAsync(this SessionFixture session) =>
        await session.DeserializeAsync<IEnumerable<Creditor>>(await session.AdministratorGetAsync("creditors"));

    public static async ValueTask<CleaningSchedule> GetCleaningScheduleAsync(this SessionFixture session) =>
        await session.DeserializeAsync<CleaningSchedule>(await session.AdministratorGetAsync("cleaning-schedule"));

    public static async ValueTask<IEnumerable<Posting>> GetPostingsAsync(this SessionFixture session, LocalDate date) =>
        await session.DeserializeAsync<IEnumerable<Posting>>(await session.AdministratorGetAsync($"postings?month={GetMonth(date)}"));

    static string GetMonth(LocalDate date) =>
        date.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "-01";

    public static async ValueTask<Debtor> PayInAsync(this SessionFixture session, PaymentId paymentId, Amount amount)
    {
        var request = new PayInRequest { Date = LocalDate.FromDateTime(DateTime.UtcNow), Amount = amount };
        return await session.DeserializeAsync<Debtor>(await session.AdministratorPostAsync($"payments/{paymentId}", request));
    }

    public static async ValueTask<Creditor> PayOutAsync(this SessionFixture session, UserId userId, Amount amount)
    {
        var request = new PayOutRequest { Date = LocalDate.FromDateTime(DateTime.UtcNow), Amount = amount };
        return await session.DeserializeAsync<Creditor>(await session.AdministratorPostAsync($"users/{userId}/pay-out", request));
    }

    public static async ValueTask<OrderDetails> SettleReservationAsync(
        this SessionFixture session, UserId userId, OrderId orderId, ReservationIndex reservationIndex)
    {
        var request = new SettleReservationRequest { UserId = userId, ReservationId = new(orderId, reservationIndex) };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPostAsync($"orders/user/{orderId}/settle-reservation", request));
    }

    public static async ValueTask<OrderDetails> SettleReservationAsync(
        this SessionFixture session, UserId userId, OrderId orderId, ReservationIndex reservationIndex, Amount damages, string description)
    {
        var request = new SettleReservationRequest
        {
            UserId = userId,
            ReservationId = new(orderId, reservationIndex),
            Damages = damages,
            Description = description
        };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPostAsync($"orders/user/{orderId}/settle-reservation", request));
    }

    public static async ValueTask<OrderDetails> CancelReservationAsync(
        this SessionFixture session, UserId userId, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        var request = new UpdateUserOrderRequest { UserId = userId, CancelledReservations = reservationIndices.ToHashSet() };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/user/{orderId}", request));
    }

    public static async ValueTask<OrderDetails> UpdateAccountNumberAsync(
        this SessionFixture session, UserId userId, OrderId orderId, string accountNumber)
    {
        var request = new UpdateUserOrderRequest { UserId = userId, AccountNumber = accountNumber};
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/user/{orderId}", request));
    }

    public static async ValueTask<OrderDetails> AllowUserToCancelWithoutFee(this SessionFixture session, UserId userId, OrderId orderId)
    {
        var request = new UpdateUserOrderRequest { UserId = userId, AllowCancellationWithoutFee = true };
        return await session.DeserializeAsync<OrderDetails>(await session.AdministratorPatchAsync($"orders/user/{orderId}", request));
    }
}
