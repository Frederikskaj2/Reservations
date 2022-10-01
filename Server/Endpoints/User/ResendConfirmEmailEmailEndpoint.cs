using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class ResendConfirmEmailEmailEndpoint : EndpointBaseAsync.WithoutRequest.WithoutResult
{
    readonly Func<Instant> getTimestamp;
    readonly ILogger logger;
    readonly Func<ResendConfirmEmailEmailCommand, EitherAsync<Failure, Unit>> sendConfirmEmailEmail;

    public ResendConfirmEmailEmailEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<ResendConfirmEmailEmailEndpoint> logger)
    {
        this.logger = logger;
        getTimestamp = () => dateProvider.Now;
        sendConfirmEmailEmail = command => ResendConfirmEmailEmailHandler.Handle(contextFactory, emailService, command);
    }

    [HttpPost("user/resend-confirm-email-email")]
    public override Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new ResendConfirmEmailEmailCommand(getTimestamp(), userId)
            from _ in sendConfirmEmailEmail(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
