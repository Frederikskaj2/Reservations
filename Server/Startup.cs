using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.EmailSender;
using Frederikskaj2.Reservations.Infrastructure;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Frederikskaj2.Reservations.Server;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTransient<IConfigureOptions<JsonOptions>, ConfigureJsonOptions>()
            .AddTransient<DatabaseInitializer>()
            .AddTransient<SeedData>()
            .AddShared()
            .AddSerialization()
            .AddInfrastructure()
            .AddCosmos()
            .AddApplication(Configuration)
            .AddEmail();

        services
            .AddOption<AuthenticationOptions>()
            .AddOption<CookieOptions>()
            .AddOption<CosmosOptions>()
            .AddOption<EmailApiOptions>()
            .AddOption<EmailQueueOptions>()
            .AddOption<EmailMessageOptions>()
            .AddOption<EmailSenderOptions>()
            .AddOption<TestingOptions>()
            .AddTransient<IOptionsFactory<OrderingOptions>, OrderingOptionsFactory>();

        services
            .AddJwtAuthentication()
            .AddCookie();

        services
            .AddControllers(options => options.Filters.Add(typeof(ExceptionFilter)));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
        });
    }
}
