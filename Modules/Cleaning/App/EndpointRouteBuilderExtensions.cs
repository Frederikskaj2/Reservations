using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Cleaning;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCleaningEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        builder.MapGet("cleaning-schedule", GetCleaningScheduleEndpoint.Handle);
        builder.MapPost("cleaning-schedule/send", SendCleaningScheduleEndpoint.Handle);
        if (isDevelopment)
        {
            builder.MapPost("jobs/send-cleaning-schedule-update/run", SendCleaningScheduleUpdateEndpoint.Handle);
            builder.MapPost("jobs/update-cleaning-schedule/run", UpdateCleaningScheduleEndpoint.Handle);
        }
        return builder;
    }
}
