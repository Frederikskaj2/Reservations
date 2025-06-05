using Microsoft.Extensions.DependencyInjection;
using System;

namespace Frederikskaj2.Reservations.Core;

public static class ServiceCollectionExtensions
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
}
