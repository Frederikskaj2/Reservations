using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class PayInEndpoint : EndpointBaseAsync.WithRequest<PayInServerRequest>.WithActionResult<Debtor>
{
    readonly ILogger logger;
    readonly Func<PayInCommand, EitherAsync<Failure, Debtor>> payIn;
    readonly Func<string?, PayInRequest?, UserId, EitherAsync<Failure, PayInCommand>> validateRequest;

    public PayInEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<PayInEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (paymentId, request, createdByUserId) => Validator.ValidatePayIn(dateProvider, paymentId, request, createdByUserId).ToAsync();
        payIn = command => PayInHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
    }

    [HttpPost("payments/{paymentId}")]
    public override Task<ActionResult<Debtor>> HandleAsync([FromRoute] PayInServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from createdByUserId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(request.PaymentId, request.ClientRequest, createdByUserId)
            from order in payIn(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
