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

public class NewPasswordEndpoint : EndpointBaseAsync.WithRequest<NewPasswordRequest>.WithoutResult
{
    readonly ILogger logger;
    readonly Func<NewPasswordCommand, EitherAsync<Failure<NewPasswordError>, Unit>> newPassword;
    readonly Func<NewPasswordRequest, EitherAsync<Failure<NewPasswordError>, NewPasswordCommand>> validateRequest;

    public NewPasswordEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<NewPasswordEndpoint> logger, IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator, ITokenProvider tokenProvider)
    {
        this.logger = logger;
        validateRequest = request => Validator.ValidateNewPassword(dateProvider, request).ToAsync();
        newPassword = model => NewPasswordHandler.Handle(contextFactory, passwordHasher, passwordValidator, tokenProvider, model);
    }

    [HttpPost("user/new-password")]
    public override Task<ActionResult> HandleAsync(NewPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from command in validateRequest(request)
            from _ in newPassword(command)
            select unit;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
