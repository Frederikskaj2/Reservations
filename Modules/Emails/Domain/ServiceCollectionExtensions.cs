using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Emails;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailsDomain(this IServiceCollection services) =>
        services
            .AddOption<EmailMessageOptions>()
            .AddOption<EmailSenderOptions>()
            .AddScoped(serviceProvider => new HtmlRenderer(serviceProvider, serviceProvider.GetRequiredService<ILoggerFactory>()))
            .AddScoped<EmailSender>()
            .AddScoped<MessageFactory>();
}
