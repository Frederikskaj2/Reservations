using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public class GetReservedDaysEndpoint : EndpointBaseAsync.WithRequest<DateRangeServerRequest>.WithActionResult<IEnumerable<MyReservedDay>>
{
    readonly Func<ReservedDaysCommand, EitherAsync<Failure, IEnumerable<MyReservedDay>>> getReservedDays;
    readonly ILogger logger;
    readonly Func<string?, string?, EitherAsync<Failure, ReservedDaysCommand>> validateReservedDays;

    public GetReservedDaysEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetReservedDaysEndpoint> logger)
    {
        this.logger = logger;
        validateReservedDays = (fromDate, toDate) => Validator.ValidateReservedDays(fromDate, toDate).ToAsync();
        getReservedDays = command => GetReservedDaysHandler.Handle(contextFactory, command);
    }

    [Route("reserved-days")]
    public override Task<ActionResult<IEnumerable<MyReservedDay>>> HandleAsync(
        [FromQuery] DateRangeServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from command in validateReservedDays(request.FromDate, request.ToDate)
            from reservedDays in getReservedDays(command)
            select reservedDays;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
