﻿using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.GetPayOutsShell;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;

namespace Frederikskaj2.Reservations.Bank;

class GetPayOutsEndpoint
{
    GetPayOutsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetPayOutsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from payOuts in GetPayOuts(entityReader, httpContext.RequestAborted)
            select new GetPayOutsResponse(CreatePayOuts(payOuts));
        return either.ToResult(logger, httpContext);
    }
}
