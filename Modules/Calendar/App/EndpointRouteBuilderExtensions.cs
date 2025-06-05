using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Calendar;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("reserved-days", GetReservedDaysEndpoint.Handle);
        builder.MapGet("reserved-days/my", GetMyReservedDaysEndpoint.Handle);
        builder.MapGet("reserved-days/owner", GetOwnerReservedDaysEndpoint.Handle);
        return builder;
    }
}
