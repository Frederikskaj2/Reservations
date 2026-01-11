using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services) =>
        services
            .AddMemoryCache()
            .AddTransient<IConfigureOptions<MemoryCacheOptions>, ConfigureMemoryCacheOptions>()
            .AddSingleton<JsonConverter, AccountAmountsConverter>()
            .AddSingleton<JsonConverter, AccountNumberConverter>()
            .AddSingleton<JsonConverter, DeviceIdConverter>()
            .AddSingleton<JsonConverter, TokenIdConverter>()
            .AddSingleton<IPasswordHasher, PasswordHasher>()
            .AddSingleton<IPasswordValidator, PasswordValidator>()
            .AddSingleton<ITokenValidator, TokenValidator>()
            .AddSingleton<EncryptedTokenProvider>()
            .AddSingleton<TokenFactory>()
            .AddSingleton<RandomNumberGenerator>()
            .AddSingleton<RemotePasswordChecker>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IDeviceCookieService, DeviceCookieService>()
            .AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>()
            .AddScoped<IUsersEmailService, UsersEmailService>()
            .AddScoped<CookieValueSerializer>()
            .AddOption<CookieOptions>()
            .AddOption<PasswordOptions>()
            .AddOption<TokenEncryptionOptions>()
            .AddOption<TokensOptions>()
            .AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>()
            .AddJob<DeleteUsersJobRegistration, DeleteUsersJob>();

    class ConfigureMemoryCacheOptions(IOptions<PasswordOptions> options) : IConfigureOptions<MemoryCacheOptions>
    {
        readonly PasswordOptions passwordOptions = options.Value;

        public void Configure(MemoryCacheOptions options) => options.SizeLimit = passwordOptions.RemoteChecker.CacheSize;
    }
}
