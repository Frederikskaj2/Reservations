using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class ReimburseEndpoint : EndpointBaseAsync.WithRequest<ReimburseServerRequest>.WithActionResult
{
    readonly ILogger logger;
    readonly Func<ReimburseCommand, EitherAsync<Failure, Unit>> reimburse;
    readonly Func<UserId, ReimburseServerRequest, UserId, EitherAsync<Failure, ReimburseCommand>> validateRequest;

    public ReimburseEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<ReimburseEndpoint> logger)
    {
        this.logger = logger;
        validateRequest =
            (userId, request, administratorUserId) => Validator.ValidateReimburse(dateProvider, userId, request.Body, administratorUserId).ToAsync();
        reimburse = command => ReimburseHandler.Handle(command, contextFactory);
    }

    [HttpPost("users/{userId:int}/reimburse")]
    public override Task<ActionResult> HandleAsync([FromRoute] ReimburseServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from administratorUserId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(UserId.FromInt32(request.UserId), request, administratorUserId)
            from _ in reimburse(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
