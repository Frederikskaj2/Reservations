using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Core;

static class GetConfigurationEndpoint
{
    public static Task<IResult> Handle([FromServices] IOptionsSnapshot<OrderingOptions> options) =>
        Task.FromResult<IResult>(!(options.Value.Testing?.IsConfigurationUnavailable ?? false)
            ? TypedResults.Ok(
                new GetConfigurationResponse(
                    options.Value,
                    Resources.All,
                    Apartments.All.Filter(apartment => apartment != Apartment.Deleted),
                    AccountNames.All))
            : TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable));
}
