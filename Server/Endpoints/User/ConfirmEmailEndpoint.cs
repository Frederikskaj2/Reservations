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

public class ConfirmEmailEndpoint : EndpointBaseAsync.WithRequest<ConfirmEmailRequest>.WithoutResult
{
    readonly Func<ConfirmEmailCommand, EitherAsync<Failure<ConfirmEmailError>, Unit>> confirmEmail;
    readonly ILogger logger;
    readonly Func<ConfirmEmailRequest, EitherAsync<Failure<ConfirmEmailError>, ConfirmEmailCommand>> validateRequest;

    public ConfirmEmailEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<ConfirmEmailEndpoint> logger, ITokenProvider tokenProvider)
    {
        this.logger = logger;
        validateRequest = request => Validator.ValidateConfirmEmail(dateProvider, request).ToAsync();
        confirmEmail = command => ConfirmEmailHandler.Handle(contextFactory, tokenProvider, command);
    }

    [HttpPost("user/confirm-email")]
    public override Task<ActionResult> HandleAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var either =
            from command in validateRequest(request)
            from _ in confirmEmail(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext);
    }
}
