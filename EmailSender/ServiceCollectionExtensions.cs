using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Frederikskaj2.Reservations.EmailSender;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services
            .AddHostedService<EmailService>()
            .AddSingleton<EmailQueue>()
            .AddSingleton<MessageFactory>()
            .AddSingleton<TemplateEngine>()
            .AddSingleton(CreateCultureInfo());
        services.AddHttpClient<EmailApiService>(options => options.DefaultRequestHeaders.Add("Accept", "application/json"));
        return services;
    }

    static CultureInfo CreateCultureInfo() =>
        new("da-DK")
        {
            NumberFormat =
            {
                CurrencyPositivePattern = 2,
                CurrencyNegativePattern = 12
            }
        };
}
