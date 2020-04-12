using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Controllers;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<ReservationsContext>(options => options.UseSqlite("Data Source=Reservations.db"))
                .Configure<SeedDataOptions>(Configuration.GetSection("SeedData"))
                .AddScoped<SeedData>();

            services
                .Configure<IdentityOptions>(
                    options =>
                    {
                        options.Lockout.MaxFailedAccessAttempts = 10;

                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredLength = 4;
                        options.Password.RequiredUniqueChars = 3;

                        options.User.RequireUniqueEmail = true;
                    })
                .Configure<SecurityStampValidatorOptions>(
                    options => options.ValidationInterval = TimeSpan.FromMinutes(10))
                .Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromDays(1))
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<ReservationsContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(
                options =>
                {
                    options.Cookie.HttpOnly = false;
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                });

            services
                .AddReservationsServices()
                .AddScoped<IDataProvider, ServerDataProvider>()
                .AddScoped<OrderService>()
                .AddEmail(Configuration)
                .AddResponseCompression(
                    options
                        => options.MimeTypes =
                            ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

            services
                .AddControllers()
                .AddNewtonsoftJson(
                    options =>
                    {
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    });
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is called by convention and cannot be static.")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Program>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
                });
        }
    }
}