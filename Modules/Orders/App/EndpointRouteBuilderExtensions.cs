using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Orders;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        builder.MapGet("creditors", GetCreditorsEndpoint.Handle);
        builder.MapGet("orders", GetOrdersEndpoint.Handle);
        builder.MapGet("orders/{orderId:int}", GetOrderEndpoint.Handle);
        builder.MapGet("orders/my", GetMyOrdersEndpoint.Handle);
        builder.MapPost("orders/my", PlaceMyOrderEndpoint.Handle);
        builder.MapGet("orders/my/{orderId:int}", GetMyOrderEndpoint.Handle);
        builder.MapPatch("orders/my/{orderId:int}", UpdateMyOrderEndpoint.Handle);
        builder.MapPost("orders/owner", PlaceOwnerOrderEndpoint.Handle);
        builder.MapPatch("orders/owner/{orderId:int}", UpdateOwnerOrderEndpoint.Handle);
        builder.MapPost("orders/resident", PlaceResidentOrderEndpoint.Handle);
        builder.MapPatch("orders/resident/{orderId:int}", UpdateResidentOrderEndpoint.Handle);
        builder.MapPatch("orders/resident/{orderId:int}/reservations", UpdateResidentReservationsEndpoint.Handle);
        builder.MapPost("orders/resident/{orderId:int}/settle-reservation", SettleReservationEndPoint.Handle);
        builder.MapGet("reports/yearly-summary", GetYearlySummaryEndpoint.Handle);
        builder.MapGet("reports/yearly-summary/range", GetYearlySummaryRangeEndpoint.Handle);
        builder.MapGet("residents", GetResidentsEndpoint.Handle);
        builder.MapGet("transactions", GetMyTransactionsEndpoint.Handle);
        builder.MapGet("users/{userId:int}/transactions", GetUserTransactionsEndpoint.Handle);
        builder.MapPost("users/{userId:int}/reimburse", ReimburseEndpoint.Handle);
        if (isDevelopment)
        {
            builder.MapPost("jobs/confirm-orders/run", ConfirmOrdersEndpoint.Handle);
            builder.MapPost("jobs/finish-owner-orders/run", FinishOwnerOrdersEndpoint.Handle);
            builder.MapPost("jobs/remove-account-numbers/run", RemoveAccountNumbersEndpoint.Handle);
            builder.MapPost("jobs/send-lock-box-codes/run", SendLockBoxCodesEndpoint.Handle);
            builder.MapPost("jobs/send-settlement-needed-reminders/run", SendSettlementNeededRemindersEndpoint.Handle);
        }
        return builder;
    }
}
