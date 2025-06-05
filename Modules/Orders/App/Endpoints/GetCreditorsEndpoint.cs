using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.CreditorFactory;
using static Frederikskaj2.Reservations.Orders.GetCreditorsShell;

namespace Frederikskaj2.Reservations.Orders;

class GetCreditorsEndpoint
{
    GetCreditorsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetCreditorsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from creditors in GetCreditors(entityReader, httpContext.RequestAborted)
            select new GetCreditorsResponse(creditors.Map(CreateCreditor));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
