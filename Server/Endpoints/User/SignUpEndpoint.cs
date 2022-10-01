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

namespace Frederikskaj2.Reservations.Server;

public class SignUpEndpoint : EndpointBaseAsync.WithRequest<SignUpRequest>.WithoutResult
{
    readonly ILogger logger;
    readonly Func<SignUpCommand, EitherAsync<Failure<SignUpError>, Unit>> signUp;
    readonly Func<SignUpRequest, EitherAsync<Failure<SignUpError>, SignUpCommand>> validateRequest;

    public SignUpEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<SignUpEndpoint> logger,
        IPasswordHasher passwordHasher, IPasswordValidator passwordValidator)
    {
        this.logger = logger;
        validateRequest = request => Validator.ValidateSignUp(Apartments.IsValid, dateProvider, request).ToAsync();
        signUp = command => SignUpHandler.Handle(contextFactory, emailService, passwordHasher, passwordValidator, command);
    }

    [HttpPost("/user/sign-up")]
    public override Task<ActionResult> HandleAsync(SignUpRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from command in validateRequest(request)
            from _ in signUp(command)
            select _;
        return either
            .Do(_ => logger.LogInformation("User with email {Email} signed up", request.Email.MaskEmail()))
            .ToResultAsync(logger, HttpContext, true);
    }
}
