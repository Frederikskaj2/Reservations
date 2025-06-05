using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.LockBox;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLockBoxEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        builder.MapGet("lock-box-codes", GetLockBoxCodesEndpoint.Handle);
        builder.MapPost("lock-box-codes/send", SendLockBoxCodesOverviewEndpoint.Handle);
        if (isDevelopment)
            builder.MapPost("jobs/update-lock-box-codes/run", UpdateLockBoxCodesEndpoint.Handle);
        return builder;
    }
}
