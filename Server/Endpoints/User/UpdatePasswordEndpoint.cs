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
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize]
public class UpdatePasswordEndpoint : EndpointBaseAsync.WithRequest<UpdatePasswordServerRequest>.WithActionResult<Tokens>
{
    readonly Func<AuthenticatedUser, Tokens> createTokens;
    readonly ILogger logger;
    readonly Func<string?, EitherAsync<Failure<PasswordError>, ParsedRefreshToken>> parseRefreshToken;
    readonly Func<UpdatePasswordCommand, EitherAsync<Failure<PasswordError>, AuthenticatedUser>> updatePassword;
    readonly Func<UpdatePasswordRequest?, ParsedRefreshToken, EitherAsync<Failure<PasswordError>, UpdatePasswordCommand>> validateRequest;

    public UpdatePasswordEndpoint(
        AuthenticationService authenticationService, IPersistenceContextFactory contextFactory, IDateProvider dateProvider,
        ILogger<UpdatePasswordEndpoint> logger, IOptionsSnapshot<AuthenticationOptions> options, IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator, RefreshTokenCookieService refreshTokenCookieService)
    {
        this.logger = logger;
        parseRefreshToken = cookie => refreshTokenCookieService.ParseCookie(cookie, PasswordError.Unknown);
        validateRequest = (request, token) => Validator.ValidateUpdatePassword(dateProvider, request, token).ToAsync();
        updatePassword = command => UpdatePasswordHandler.Handle(contextFactory, options.Value, passwordHasher, passwordValidator, command);
        createTokens = authenticationService.CreateTokens;
    }

    [HttpPost("user/update-password")]
    public override Task<ActionResult<Tokens>> HandleAsync([FromRoute] UpdatePasswordServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from parsedRefreshToken in parseRefreshToken(request.Cookie)
            from command in validateRequest(request.ClientRequest, parsedRefreshToken)
            from user in updatePassword(command)
            let tokens = createTokens(user)
            select tokens;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
