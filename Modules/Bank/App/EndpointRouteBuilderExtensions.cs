﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Bank;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapBankEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        builder.MapGet("bank/pay-outs", GetPayOutsEndpoint.Handle);
        builder.MapPost("bank/pay-outs", CreatePayOutEndpoint.Handle);
        builder.MapDelete("bank/pay-outs/{payOutId:int}", DeletePayOutEndpoint.Handle);
        builder.MapGet("bank/transactions", GetBankTransactionsEndpoint.Handle);
        builder.MapPatch("bank/transactions/{bankTransactionId:int}", UpdateBankTransactionEndpoint.Handle);
        builder.MapPost("bank/transactions/{bankTransactionId:int}/reconcile/{paymentId}", ReconcileEndpoint.Handle);
        builder.MapPost("bank/transactions/{bankTransactionId:int}/reconcile-pay-out/{payOutId:int}", ReconcilePayOutEndpoint.Handle);
        builder.MapPost("bank/transactions/import", ImportBankTransactionsEndpoint.Handle);
        builder.MapGet("bank/transactions/range", GetBankTransactionsRangeEndpoint.Handle);
        builder.MapGet("postings", GetPostingsEndpoint.Handle);
        builder.MapGet("postings/range", GetPostingsRangeEndpoint.Handle);
        builder.MapPost("postings/send", SendPostingsEndpoint.Handle);
        if (isDevelopment)
            builder.MapPost("jobs/send-debt-reminders/run", SendDebtRemindersEndpoint.Handle);
        return builder;
    }
}
