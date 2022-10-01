using Frederikskaj2.Reservations.Application.BackgroundServices;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddConfiguration(configuration)
            .AddServices()
            .AddHostedServices()
            .AddJsonConverters();

    static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        services
            .Configure<ApplicationOptions>(configuration.GetSection("Application"))
            .Configure<AuthenticationOptions>(configuration.GetSection("Authentication"))
            .Configure<EmailQueueOptions>(configuration.GetSection("EmailQueue"))
            .Configure<PasswordOptions>(configuration.GetSection("Password"))
            .Configure<ScheduledEmailOptions>(configuration.GetSection("ScheduledEmail"))
            .Configure<TokenEncryptionOptions>(configuration.GetSection("TokenEncryption"))
            .AddTransient<IConfigureOptions<EmailQueueOptions>, ConfigureEmailQueueOptions>();

    static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<IEmailService, EmailService>()
            .AddSingleton<IFormatter, Formatter>()
            .AddSingleton<ITokenProvider, TokenProvider>();

    static IServiceCollection AddHostedServices(this IServiceCollection services) =>
        services
            .AddHostedService<SchedulingService<ScheduledEmailService>>()
            .AddScoped<CleaningScheduleService>()
            .AddScoped<DebtReminderService>()
            .AddScoped<LockBoxCodesService>()
            .AddScoped<OrderService>()
            .AddScoped<ScheduledEmailService>();

    // Make this method available to Importer.
    internal static IServiceCollection AddJsonConverters(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, AccountAmountsConverter>()
            .AddSingleton<JsonConverter, HashMapConverter>()
            .AddSingleton<JsonConverter, SeqByteConverter>()
            .AddSingleton<JsonConverter, SeqConverter>();

    class ConfigureEmailQueueOptions : IConfigureOptions<EmailQueueOptions>
    {
        readonly JsonSerializerOptions serializerOptions;

        public ConfigureEmailQueueOptions(IOptions<JsonSerializerOptions> serializerOptions) =>
            this.serializerOptions = serializerOptions.Value;

        public void Configure(EmailQueueOptions options) =>
            options.SerializerOptions = serializerOptions;
    }
}
