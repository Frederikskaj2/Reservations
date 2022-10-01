using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Frederikskaj2.Reservations.Server;

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOption<TOption>(this IServiceCollection services) where TOption : class
    {
        services.AddOptions<TOption>().BindConfiguration(GetSectionName());
        return services;

        static string GetSectionName()
        {
            const string suffix = "Options";
            var name = typeof(TOption).Name;
            return name.EndsWith(suffix, StringComparison.Ordinal) ? name[..^suffix.Length] : name;
        }
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddOption<TokensOptions>()
            .AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>()
            .AddScoped<AuthenticationService>()
            .AddAuthorizationCore(options =>
            {
                options.AddPolicy(
                    Policies.ViewOrders,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
                options.AddPolicy(
                    Policies.ViewUsers,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
            })
            .AddAuthenticationCore(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddWebEncoders()
            .TryAddSingleton<ISystemClock, SystemClock>();
        new AuthenticationBuilder(services).AddJwtBearer();
        return services;
    }

    public static IServiceCollection AddCookie(this IServiceCollection services) =>
        services
            .AddScoped<CookieValueSerializer>()
            .AddScoped<DeviceCookieService>()
            .AddScoped<RefreshTokenCookieService>();
}
