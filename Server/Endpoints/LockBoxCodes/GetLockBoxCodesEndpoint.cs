using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.LockBoxCodes))]
public class GetLockBoxCodesEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<IEnumerable<WeeklyLockBoxCodes>>
{
    readonly Func<LocalDate> getToday;
    readonly Func<LocalDate, EitherAsync<Failure, IEnumerable<WeeklyLockBoxCodes>>> getWeeklyLockBoxCodes;
    readonly ILogger logger;

    public GetLockBoxCodesEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetLockBoxCodesEndpoint> logger)
    {
        this.logger = logger;
        getToday = () => dateProvider.Today;
        getWeeklyLockBoxCodes = date => GetWeeklyLockBoxCodesHandler.Handle(contextFactory, date);
    }

    [HttpGet("lock-box-codes")]
    public override Task<ActionResult<IEnumerable<WeeklyLockBoxCodes>>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getWeeklyLockBoxCodes(getToday());
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
