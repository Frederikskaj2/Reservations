using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class NewPassword
{
    public static Either<Failure<NewPasswordError>, NewPasswordOutput> NewPasswordCore(
        IPasswordHasher passwordHasher,
        ITokenValidator tokenValidator,
        NewPasswordInput input) =>
        from _ in ParseToken(tokenValidator, input.Command)
        let hashedPassword = passwordHasher.HashPassword(input.Command.NewPassword)
        select new NewPasswordOutput(UpdateUser(input.Command.Timestamp, input.User, hashedPassword));

    static Either<Failure<NewPasswordError>, Unit> ParseToken(ITokenValidator tokenValidator, NewPasswordCommand command) =>
        tokenValidator.ValidateNewPasswordToken(command.Timestamp, command.Email, command.Token) switch
        {
            TokenValidationResult.Valid => unit,
            TokenValidationResult.Expired => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.TokenExpired, "Token is expired."),
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.InvalidRequest, "Token is invalid."),
        };

    static User UpdateUser(Instant timestamp, User user, Seq<byte> hashedPassword) =>
        user with
        {
            Security = new() { HashedPassword = hashedPassword, NextRefreshTokenId = user.Security.NextRefreshTokenId.NextId, RefreshTokens = Empty },
            Audits = user.Audits.Add(UserAudit.UpdatePassword(timestamp, user.UserId)),
        };
}
