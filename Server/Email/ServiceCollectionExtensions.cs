using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;

namespace Frederikskaj2.Reservations.Server.Email
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            var root = Directory.GetCurrentDirectory();
            var cultureInfo = new CultureInfo("da-DK")
            {
                NumberFormat =
                {
                    CurrencyPositivePattern = 2,
                    CurrencyNegativePattern = 12
                }
            };
            services
                .Configure<MvcRazorRuntimeCompilationOptions>(options => options.FileProviders.Add(new PhysicalFileProvider(root)))
                .Configure<EmailOptions>(configuration.GetSection("Email"))
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton(typeof(IBackgroundWorkQueue<>), typeof(BackgroundWorkQueue<>))
                .AddHostedService<BackgroundWorkerService<IEmailService>>()
                .AddHostedService<SchedulingService<ScheduledEmailService>>()
                .AddScoped<RazorViewToStringRenderer>()
                .AddScoped<UrlService>()
                .AddScoped<ScheduledEmailService>()
                .AddSingleton(cultureInfo)
                .AddRazorPages();
            services.AddHttpClient<IEmailService, EmailService>(options => options.DefaultRequestHeaders.Add("Accept", "application/json"));
            return services;
        }
    }
}