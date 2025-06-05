using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Server;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = IsDevelopmentEnvironment(builder.Environment);

if (isDevelopment)
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.AddTelemetry(builder.Environment);

builder.Services.AddProblemDetails();

builder.Services
    .AddBankInfrastructure()
    .AddBankSharedDomain()
    .AddCleaningInfrastructure()
    .AddOrdersInfrastructure()
    .AddOrdersDomain()
    .AddOrdersSharedDomain()
    .AddLockBoxInfrastructure()
    .AddLockBoxSharedDomain()
    .AddUsersInfrastructure()
    .AddUsersDomain()
    .AddUsersSharedDomain()
    .AddEmailsInfrastructure()
    .AddEmailsDomain()
    .AddPersistenceInfrastructure()
    .AddCoreApp()
    .AddCoreInfrastructure()
    .AddCoreSharedInfrastructure()
    .AddCoreSharedDomain();

builder.Services.AddJwtAuthentication();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception switch
    {
        BadHttpRequestException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError,
    },
});

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app
    .MapBankEndpoints(isDevelopment)
    .MapCalendarEndpoints()
    .MapCleaningEndpoints(isDevelopment)
    .MapCoreEndpoints()
    .MapEmailsEndpoints(isDevelopment)
    .MapLockBoxEndpoints(isDevelopment)
    .MapOrdersEndpoints(isDevelopment)
    .MapUsersEndpoints(isDevelopment)
    .MapFallbackToFile("index.html");

await app.RunAsync();

static bool IsDevelopmentEnvironment(IHostEnvironment hostEnvironment) =>
    hostEnvironment.EnvironmentName is not "Production" and not "Test";

public partial class Program
{
    Program() { }
}
