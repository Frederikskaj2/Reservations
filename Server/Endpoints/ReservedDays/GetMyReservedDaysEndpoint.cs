using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Resident))]
public class GetMyReservedDaysEndpoint : EndpointBaseAsync.WithRequest<DateRangeServerRequest>.WithActionResult<IEnumerable<MyReservedDay>>
{
    readonly Func<UserReservedDaysCommand, EitherAsync<Failure, IEnumerable<MyReservedDay>>> getReservedDays;
    readonly ILogger logger;
    readonly Func<string?, string?, UserId, EitherAsync<Failure, UserReservedDaysCommand>> validateReservedDays;

    public GetMyReservedDaysEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetMyReservedDaysEndpoint> logger)
    {
        this.logger = logger;
        validateReservedDays = (fromDate, toDate, userId) => Validator.ValidateUserReservedDays(fromDate, toDate, userId).ToAsync();
        getReservedDays = command => GetMyReservedDaysHandler.Handle(contextFactory, command);
    }

    [Route("reserved-days/my")]
    public override Task<ActionResult<IEnumerable<MyReservedDay>>> HandleAsync([FromQuery] DateRangeServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateReservedDays(request.FromDate, request.ToDate, userId)
            from reservedDays in getReservedDays(command)
            select reservedDays;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
