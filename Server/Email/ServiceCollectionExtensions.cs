using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage(
            "Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The dependency injection container will dispose the instance.")]
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            var root = Directory.GetCurrentDirectory();
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            var cultureInfo = new CultureInfo("da-DK")
            {
                NumberFormat =
                {
                    CurrencyPositivePattern = 2,
                    CurrencyNegativePattern = 12
                }
            };
            services
                .Configure<MvcRazorRuntimeCompilationOptions>(
                    options => options.FileProviders.Add(new PhysicalFileProvider(root)))
                .Configure<EmailOptions>(configuration.GetSection("Email"))
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton(diagnosticSource)
                .AddSingleton<DiagnosticSource>(diagnosticSource)
                .AddSingleton(typeof(IBackgroundWorkQueue<>), typeof(BackgroundWorkQueue<>))
                .AddHostedService<BackgroundWorkerService<EmailService>>()
                .AddScoped<RazorViewToStringRenderer>()
                .AddScoped<UrlService>()
                .AddScoped<EmailService>()
                .AddSingleton(cultureInfo)
                .AddRazorPages();
            return services;
        }
    }
}