using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class GetPostingRangeEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<PostingsRange>
{
    readonly Func<EitherAsync<Failure, PostingsRange>> getPostingsRange;
    readonly ILogger logger;

    public GetPostingRangeEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<GetPostingRangeEndpoint> logger)
    {
        this.logger = logger;
        getPostingsRange = () => GetPostingsRangeHandler.Handle(contextFactory, dateProvider);
    }

    [HttpGet("postings/range")]
    public override Task<ActionResult<PostingsRange>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either = getPostingsRange();
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
