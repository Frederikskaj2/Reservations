using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.GetResidentsShell;
using static Frederikskaj2.Reservations.Orders.ResidentFactory;

namespace Frederikskaj2.Reservations.Orders;

class GetResidentsEndpoint
{
    GetResidentsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetResidentsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from users in GetResidents(entityReader, httpContext.RequestAborted)
            select new GetResidentsResponse(users.Map(CreateResident));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
