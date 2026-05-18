using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.RoomAccess;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapRoomAccessEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        if (isDevelopment)
        {
            builder.MapPost("jobs/send-room-entry-codes/run", SendRoomEntryCodesEndpoint.Handle);
            builder.MapPost("jobs/update-smart-lock-authorizations/run", UpdateSmartLockAuthorizationsEndpoint.Handle);
        }
        return builder;
    }
}
