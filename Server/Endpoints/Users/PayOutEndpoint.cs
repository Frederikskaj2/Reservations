using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class PayOutEndpoint : EndpointBaseAsync.WithRequest<PayOutServerRequest>.WithActionResult<Creditor>
{
    readonly ILogger logger;
    readonly Func<PayOutCommand, EitherAsync<Failure, Creditor>> payOut;
    readonly Func<UserId, PayOutRequest?, UserId, EitherAsync<Failure, PayOutCommand>> validateRequest;

    public PayOutEndpoint(IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<PayOutEndpoint> logger)
    {
        this.logger = logger;
        validateRequest = (userId, request, administratorUserId) => Validator.ValidatePayOut(dateProvider, userId, request, administratorUserId).ToAsync();
        payOut = command => PayOutHandler.Handle(contextFactory, emailService, command);
    }

    [HttpPost("users/{userId:int}/pay-out")]
    public override Task<ActionResult<Creditor>> HandleAsync([FromRoute] PayOutServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from administratorUserId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(UserId.FromInt32(request.UserId), request.Body, administratorUserId)
            from debit in payOut(command)
            select debit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
