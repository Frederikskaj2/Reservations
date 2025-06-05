using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class AdministratorExtensions
{
    public static async ValueTask UpdateLockBoxCodes(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/update-lock-box-codes/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask<GetUserResponse> GetUser(this SessionFixture session, UserId userId) =>
        await session.Deserialize<GetUserResponse>(await session.AdministratorGet($"users/{userId}"));

    public static async ValueTask<UpdateUserResponse> UpdateUser(this SessionFixture session, UserId userId, string fullName, string phone, Roles roles)
    {
        var request = new UpdateUserRequest(fullName, phone, roles, IsPendingDelete: false);
        return await session.Deserialize<UpdateUserResponse>(await session.AdministratorPatch($"users/{userId}", request));
    }

    public static async ValueTask<GetUserTransactionsResponse> GetResidentTransactions(this SessionFixture session, UserId userId) =>
        await session.Deserialize<GetUserTransactionsResponse>(await session.AdministratorGet($"users/{userId}/transactions"));

    public static async ValueTask<GetOrdersResponse> GetOwnerOrders(this SessionFixture session) =>
        await session.Deserialize<GetOrdersResponse>(await session.AdministratorGet("orders?type=Owner"));

    public static async ValueTask<GetOrderResponse> GetOrder(this SessionFixture session, OrderId orderId) =>
        await session.Deserialize<GetOrderResponse>(await session.AdministratorGet($"orders/{orderId}"));

    public static async ValueTask<GetResidentsResponse> GetResidents(this SessionFixture session) =>
        await session.Deserialize<GetResidentsResponse>(await session.AdministratorGet("residents"));

    public static async ValueTask<ResidentDto> GetMyResident(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.GetResidents();
        return response.Residents.Single(resident => resident.UserIdentity.Email == session.User.Email);
    }

    public static async ValueTask<GetCreditorsResponse> GetCreditors(this SessionFixture session) =>
        await session.Deserialize<GetCreditorsResponse>(await session.AdministratorGet("creditors"));

    public static async ValueTask<GetCleaningScheduleResponse> GetCleaningSchedule(this SessionFixture session) =>
        await session.Deserialize<GetCleaningScheduleResponse>(await session.AdministratorGet("cleaning-schedule"));

    public static async ValueTask<GetPostingsResponse> GetPostings(this SessionFixture session, LocalDate date) =>
        await session.Deserialize<GetPostingsResponse>(await session.AdministratorGet($"postings?month={GetMonth(date)}"));

    static string GetMonth(LocalDate date) =>
        date.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "-01";

    public static ValueTask<SettleReservationResponse> SettleReservation(
        this SessionFixture session, OrderId orderId, ReservationIndex reservationIndex) =>
        session.SettleReservation(orderId, reservationIndex, Amount.Zero, description: null);

    public static async ValueTask<SettleReservationResponse> SettleReservation(
        this SessionFixture session, OrderId orderId, ReservationIndex reservationIndex, Amount damages, string? description)
    {
        var request = new SettleReservationRequest(reservationIndex, damages, description);
        return await session.Deserialize<SettleReservationResponse>(await session.AdministratorPost($"orders/resident/{orderId}/settle-reservation", request));
    }

    public static async ValueTask<UpdateResidentOrderResponse> CancelReservation(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices) =>
        await session.Deserialize<UpdateResidentOrderResponse>(await session.CancelReservationRaw(orderId, reservationIndices));

    public static ValueTask<HttpResponseMessage> CancelReservationRaw(
        this SessionFixture session, OrderId orderId, params ReservationIndex[] reservationIndices)
    {
        var request = new UpdateResidentOrderRequest(AccountNumber: null, reservationIndices.ToHashSet(), WaiveFee: false, AllowCancellationWithoutFee: false);
        return session.AdministratorPatch($"orders/resident/{orderId}", request);
    }

    public static async ValueTask<UpdateResidentOrderResponse> UpdateAccountNumber(this SessionFixture session, OrderId orderId, string accountNumber)
    {
        var request = new UpdateResidentOrderRequest(accountNumber, CancelledReservations: null, WaiveFee: false, AllowCancellationWithoutFee: false);
        return await session.Deserialize<UpdateResidentOrderResponse>(await session.AdministratorPatch($"orders/resident/{orderId}", request));
    }

    public static async ValueTask<UpdateResidentOrderResponse> AllowResidentToCancelWithoutFee(this SessionFixture session, OrderId orderId)
    {
        var request = new UpdateResidentOrderRequest(AccountNumber: null, CancelledReservations: null, WaiveFee: false, AllowCancellationWithoutFee: true);
        return await session.Deserialize<UpdateResidentOrderResponse>(await session.AdministratorPatch($"orders/resident/{orderId}", request));
    }

    public static async ValueTask Reimburse(this SessionFixture session, UserId userId, IncomeAccount accountToDebit, string description, Amount amount)
    {
        var request = new ReimburseRequest(LocalDate.FromDateTime(DateTime.UtcNow), accountToDebit, description, amount);
        var response = await session.AdministratorPost($"users/{userId}/reimburse", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask<GetLockBoxCodesResponse> GetLockBoxCodes(this SessionFixture session) =>
        await session.Deserialize<GetLockBoxCodesResponse>(await session.AdministratorGet("lock-box-codes"));

    public static async ValueTask<SendLockBoxCodesResponse> SendLockBoxCodes(this SessionFixture session) =>
        await session.Deserialize<SendLockBoxCodesResponse>(await session.AdministratorPost("lock-box-codes/send"));

    public static async ValueTask<GetYearlySummaryRangeResponse> GetYearlySummaryRange(this SessionFixture session) =>
        await session.Deserialize<GetYearlySummaryRangeResponse>(await session.AdministratorGet("reports/yearly-summary/range"));

    public static async ValueTask<GetYearlySummaryResponse> GetYearlySummary(this SessionFixture session, int year) =>
        await session.Deserialize<GetYearlySummaryResponse>(await session.AdministratorGet($"reports/yearly-summary?year={year}"));
}
