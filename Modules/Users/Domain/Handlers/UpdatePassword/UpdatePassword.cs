using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class UpdatePassword
{
    public static Either<Failure<PasswordError>, UpdatePasswordOutput> UpdatePasswordCore(
        AuthenticationOptions options, IPasswordHasher passwordHasher, UpdatePasswordInput input) =>
        from existingRefreshToken in GetExistingRefreshToken(input.Command.TokenId, input.User)
        from _ in VerifyCurrentPassword(passwordHasher, input.Command.CurrentPassword, input.User)
        let hashedPassword = passwordHasher.HashPassword(input.Command.NewPassword)
        let newRefreshToken = CreateNewRefreshToken(options, input.Command.Timestamp, input.User, existingRefreshToken)
        select new UpdatePasswordOutput(UpdateUser(input.Command.Timestamp, input.User, hashedPassword, newRefreshToken), newRefreshToken);

    static Either<Failure<PasswordError>, RefreshToken> GetExistingRefreshToken(TokenId tokenId, User user) =>
        user.Security.RefreshTokens.Filter(token => token.TokenId == tokenId).HeadOrNone().Case switch
        {
            RefreshToken refreshToken => refreshToken,
            _ => Failure.New(HttpStatusCode.Forbidden, PasswordError.Unknown, $"Token {tokenId} does not exist."),
        };

    static Either<Failure<PasswordError>, Unit> VerifyCurrentPassword(IPasswordHasher passwordHasher, string password, User user) =>
        passwordHasher.VerifyHashedPassword(user.Security.HashedPassword, password) != PasswordVerificationResult.Failed
            ? unit
            : Failure.New(HttpStatusCode.Forbidden, PasswordError.WrongPassword);

    static RefreshToken CreateNewRefreshToken(AuthenticationOptions options, Instant timestamp, User user, RefreshToken existingRefreshToken) =>
        existingRefreshToken with { TokenId = user.Security.NextRefreshTokenId, ExpireAt = Authentication.GetExpireAt(options, timestamp, existingRefreshToken.IsPersistent) };

    static User UpdateUser(Instant timestamp, User user, Seq<byte> hashedPassword, RefreshToken newRefreshToken) =>
        user with
        {
            Security = new()
            {
                HashedPassword = hashedPassword,
                NextRefreshTokenId = user.Security.NextRefreshTokenId.NextId,
                RefreshTokens = newRefreshToken.Cons(),
            },
            Audits = user.Audits.Add(UserAudit.UpdatePassword(timestamp, user.UserId)),
        };
}
