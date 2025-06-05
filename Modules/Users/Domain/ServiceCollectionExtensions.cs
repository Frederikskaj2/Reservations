using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Users;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersDomain(this IServiceCollection services) =>
        services
            .AddOption<AuthenticationOptions>();
}
