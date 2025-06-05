using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Users;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        return services
            .AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policy.ViewOrders,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
                options.AddPolicy(
                    Policy.ViewUsers,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
            });
    }
}
