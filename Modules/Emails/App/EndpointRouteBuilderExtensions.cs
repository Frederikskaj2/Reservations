using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Emails;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEmailsEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        if (isDevelopment)
            builder.MapPost("jobs/send-emails/run", SendEmailsEndpoint.Handle);
        return builder;
    }
}
