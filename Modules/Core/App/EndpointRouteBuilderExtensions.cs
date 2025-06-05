using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Core;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCoreEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("configuration", GetConfigurationEndpoint.Handle);
        return builder;
    }
}
