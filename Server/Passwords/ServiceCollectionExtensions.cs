using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Server.Passwords
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPasswordServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            return services
                .AddSingleton<IRandomNumberGenerator, CryptographicRandomNumberGenerator>()
                .AddSingleton<IPasswordHasher, PasswordHasher>()
                .Configure<PasswordOptions>(configuration.GetSection("Password"));
        }
    }
}