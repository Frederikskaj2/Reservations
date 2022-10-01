using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public class SendNewPasswordEmailEndpoint : EndpointBaseAsync.WithRequest<SendNewPasswordEmailRequest>.WithoutResult
{
    readonly ILogger logger;
    readonly Func<SendNewPasswordEmailCommand, EitherAsync<Failure, Unit>> sendNewPasswordEmail;
    readonly Func<SendNewPasswordEmailRequest, EitherAsync<Failure, SendNewPasswordEmailCommand>> validateRequest;

    public SendNewPasswordEmailEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<SendNewPasswordEmailEndpoint> logger)
    {
        this.logger = logger;
        validateRequest = request => Validator.ValidateSendNewPasswordEmail(dateProvider, request).ToAsync();
        sendNewPasswordEmail = command => SendNewPasswordEmail.Handle(contextFactory, emailService, command);
    }

    [HttpPost("user/send-new-password-email")]
    public override Task<ActionResult> HandleAsync(SendNewPasswordEmailRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from command in validateRequest(request)
            from _ in sendNewPasswordEmail(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
