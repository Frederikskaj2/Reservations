using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Emails;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailsInfrastructure(this IServiceCollection services)
    {
        services
            .AddOption<EmailApiOptions>()
            .AddOption<EmailQueueOptions>()
            .AddOption<EmailsOptions>()
            .AddSingleton<IEmailEnqueuer, EmailQueue>()
            .AddSingleton(serviceProvider => (IEmailDequeuer) serviceProvider.GetRequiredService<IEmailEnqueuer>())
            .AddTransient<IConfigureOptions<EmailQueueOptions>, ConfigureEmailQueueOptions>()
            .AddJob<SendEmailsJobRegistration, SendEmailsJob>();
        services.AddHttpClient<IEmailApiService, EmailApiService>(options => options.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json));
        return services;
    }

    class ConfigureEmailQueueOptions(IOptions<JsonSerializerOptions> serializerOptions) : IConfigureOptions<EmailQueueOptions>
    {
        readonly JsonSerializerOptions serializerOptions = serializerOptions.Value;

        public void Configure(EmailQueueOptions options) =>
            options.SerializerOptions = serializerOptions;
    }
}
