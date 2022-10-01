using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;
using CleaningSchedule = Frederikskaj2.Reservations.Shared.Core.CleaningSchedule;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Cleaning))]
public class GetCleaningTasksEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<CleaningSchedule>
{
    readonly Func<LocalDate, EitherAsync<Failure, CleaningSchedule>> getCleaningSchedule;
    readonly Func<LocalDate> getToday;
    readonly ILogger logger;

    public GetCleaningTasksEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetCleaningTasksEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getToday = () => dateProvider.Today;
        getCleaningSchedule = date => GetCleaningScheduleHandler.Handle(contextFactory, options.Value, date);
    }

    [HttpGet("cleaning-schedule")]
    public override Task<ActionResult<CleaningSchedule>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getCleaningSchedule(getToday());
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
