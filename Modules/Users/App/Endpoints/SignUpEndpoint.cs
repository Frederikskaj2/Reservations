using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.Validator;

namespace Frederikskaj2.Reservations.Users;

class SignUpEndpoint
{
    SignUpEndpoint() { }

    public static Task<IResult> Handle(
        [FromBody] SignUpRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IUsersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SignUpEndpoint> logger,
        [FromServices] IPasswordHasher passwordHasher,
        [FromServices] IPasswordValidator passwordValidator,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateSignUp(dateProvider, request).ToAsync()
            from _ in SignUpShell.SignUp(emailService, passwordHasher, passwordValidator, entityReader, entityWriter, command, httpContext.RequestAborted)
            select _;
        return either
            .Do(_ => logger.LogInformation("User with email {Email} signed up", request.Email.MaskEmail()))
            .ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
