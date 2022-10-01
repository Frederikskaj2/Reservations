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

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class GetPostingsEndpoint : EndpointBaseAsync.WithRequest<MonthServerRequest>.WithActionResult<IEnumerable<Posting>>
{
    readonly Func<LocalDate, EitherAsync<Failure, IEnumerable<Posting>>> getPostings;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure, LocalDate>> validateMonth;

    public GetPostingsEndpoint(IPersistenceContextFactory contextFactory, ILogger<GetPostingsEndpoint> logger)
    {
        this.logger = logger;
        validateMonth = month => Validator.ValidateMonth(month).ToAsync();
        getPostings = month => GetPostingsHandler.Handle(contextFactory, month);
    }

    [HttpGet("postings")]
    public override Task<ActionResult<IEnumerable<Posting>>> HandleAsync([FromQuery] MonthServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from date in validateMonth(request.Month)
            from postings in getPostings(date)
            select postings;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
