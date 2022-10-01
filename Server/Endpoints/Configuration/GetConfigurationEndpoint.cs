using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public class GetConfigurationEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<Configuration>
{
    readonly Configuration options;

    public GetConfigurationEndpoint(IOptionsSnapshot<OrderingOptions> options) =>
        this.options = new Configuration(
            options.Value,
            Resources.GetAll().Values,
            Apartments.GetAll().Where(apartment => apartment != Apartment.Deleted),
            AccountNames.GetAll());

    [HttpGet("configuration")]
    public override Task<ActionResult<Configuration>> HandleAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<ActionResult<Configuration>>(!(options?.Options.Testing?.IsConfigurationUnavailable ?? false)
            ? Ok(options)
            : base.StatusCode(StatusCodes.Status503ServiceUnavailable));
}
