using System;
using System.Collections.Immutable;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class UpdatePasswordFunctions
{
    public static EitherAsync<Failure<PasswordError>, IPersistenceContext> ReadUserContextUpdatePassword(IPersistenceContext context, UserId userId) =>
        MapReadErrorHideNotFoundStatus(context.ReadItem<User>(User.GetId(userId)));

    static EitherAsync<Failure<PasswordError>, T> MapReadErrorHideNotFoundStatus<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(MapReadStatusNotFoundToForbidden);

    static Failure<PasswordError> MapReadStatusNotFoundToForbidden(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => Failure.New(HttpStatusCode.Forbidden, PasswordError.WrongPassword),
            _ => Failure.New(HttpStatusCode.ServiceUnavailable, PasswordError.Unknown, $"Database read error {status}.")
        };

    public static EitherAsync<Failure<NewPasswordError>, UserEmail> ReadUserEmailNewPassword(IPersistenceContextFactory contextFactory, EmailAddress email) =>
        MapReadErrorNotFoundToUnprocessableEntity(CreateContext(contextFactory).Untracked.ReadItem<UserEmail>(EmailAddress.NormalizeEmail(email)));

    public static EitherAsync<Failure<NewPasswordError>, IPersistenceContext> ReadUserContextNewPassword(IPersistenceContext context, UserId userId) =>
        MapReadErrorNotFoundToUnprocessableEntity(context.ReadItem<User>(User.GetId(userId)));

    static EitherAsync<Failure<NewPasswordError>, T> MapReadErrorNotFoundToUnprocessableEntity<T>(EitherAsync<HttpStatusCode, T> either) =>
        either.MapLeft(MapReadStatusNotFoundToUnprocessableEntity);

    static Failure<NewPasswordError> MapReadStatusNotFoundToUnprocessableEntity(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.InvalidRequest),
            _ => Failure.New(HttpStatusCode.ServiceUnavailable, NewPasswordError.Unknown, $"Database read error {status}.")
        };

    public static EitherAsync<Failure<T>, IPersistenceContext> Write<T>(IPersistenceContext context) where T : struct, IConvertible =>
        MapWriteError<T>(context.Write());

    static EitherAsync<Failure<T>, IPersistenceContext> MapWriteError<T>(EitherAsync<HttpStatusCode, IPersistenceContext> either) where T : struct, IConvertible =>
        either.MapLeft(MapWriteStatus<T>);

    static Failure<T> MapWriteStatus<T>(HttpStatusCode status) where T : struct, IConvertible =>
        Failure.New<T>(HttpStatusCode.ServiceUnavailable, default, $"Database write error {status}.");

    public static EitherAsync<Failure<PasswordError>, RefreshToken> GetExistingRefreshToken(TokenId tokenId, User user)
    {
        var refreshToken = user.Security.RefreshTokens.FirstOrDefault(token => token.TokenId == tokenId);
        return refreshToken is not null
            ? refreshToken
            : Failure.New(HttpStatusCode.Forbidden, PasswordError.Unknown, $"Token {tokenId} does not exist.");
    }

    public static EitherAsync<Failure<PasswordError>, Unit> VerifyCurrentPassword(IPasswordHasher passwordHasher, User user, string password) =>
        passwordHasher.VerifyHashedPassword(user.Security.HashedPassword, password) != PasswordVerificationResult.Failed
            ? unit : Failure.New(HttpStatusCode.Forbidden, PasswordError.WrongPassword);

    public static EitherAsync<Failure<PasswordError>, Unit> ValidateUpdatePassword(IPasswordValidator validator, EmailAddress email, string password) =>
        validator.ValidateAsync(email, password).ToEitherAsync(PasswordValidationErrorToUpdatePasswordFailure);

    static Either<Failure<PasswordError>, Unit> PasswordValidationErrorToUpdatePasswordFailure(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, PasswordError.EmailSameAsPassword),
            _ => unit
        };

    public static EitherAsync<Failure<NewPasswordError>, Unit> ValidateNewPassword(IPasswordValidator validator, EmailAddress email, string password) =>
        validator.ValidateAsync(email, password).ToEitherAsync(PasswordValidationErrorToNewPasswordFailure);

    static Either<Failure<NewPasswordError>, Unit> PasswordValidationErrorToNewPasswordFailure(PasswordValidationError error) =>
        error switch
        {
            PasswordValidationError.TooShort => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.TooShortPassword),
            PasswordValidationError.Exposed => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.ExposedPassword),
            PasswordValidationError.SameAsEmail => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.EmailSameAsPassword),
            _ => unit
        };

    public static RefreshToken CreateNewRefreshToken(AuthenticationOptions options, Instant timestamp, User user, RefreshToken existingRefreshToken) =>
        new(
            user.Security.NextRefreshTokenId,
            Authentication.GetExpireAt(options, timestamp, existingRefreshToken.IsPersistent),
            existingRefreshToken.IsPersistent,
            existingRefreshToken.DeviceId);

    public static User UpdateUser(Instant timestamp, User user, Seq<byte> hashedPassword, RefreshToken newRefreshToken) =>
        user with
        {
            Security = new UserSecurity
            {
                HashedPassword = hashedPassword,
                NextRefreshTokenId = user.Security.NextRefreshTokenId.NextId,
                RefreshTokens = newRefreshToken.Cons()
            },
            Audits = user.Audits.Add(new(timestamp, user.UserId, UserAuditType.UpdatePassword))
        };

    public static User UpdateUser(Instant timestamp, User user, Seq<byte> hashedPassword) =>
        user with
        {
            Security = new UserSecurity
            {
                HashedPassword = hashedPassword,
                NextRefreshTokenId = user.Security.NextRefreshTokenId.NextId,
                RefreshTokens = Empty
            },
            Audits = user.Audits.Add(new(timestamp, user.UserId, UserAuditType.UpdatePassword))
        };

    public static EitherAsync<Failure, User> GetUserWithRequestNewPasswordAudit(IPersistenceContext context, Instant timestamp, EmailAddress email) =>
        from userEmail in ReadUserEmail(context, email)
        from context1 in ReadUserContext(context, userEmail.UserId)
        let user = context1.Item<User>()
        let context2 = context1.UpdateItem<User>(u => u with { Audits = u.Audits.Add(new(timestamp, u.UserId, UserAuditType.RequestNewPassword)) })
        from _1 in WriteContext(context2)
        select context2.Item<User>();

    public static EitherAsync<Failure, Unit> IgnoreNotFound(Failure failure) =>
        failure.Status switch
        {
            HttpStatusCode.NotFound => unit,
            _ => failure
        };

    public static EitherAsync<Failure<NewPasswordError>, Unit> ParseToken(ITokenProvider tokenProvider, Instant timestamp, ImmutableArray<byte> token, EmailAddress email) =>
        tokenProvider.ValidateNewPasswordToken(timestamp, email, token) switch
        {
            TokenValidationResult.Valid => unit,
            TokenValidationResult.Expired => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.TokenExpired, "Token is expired."),
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, NewPasswordError.InvalidRequest, "Token is invalid.")
        };
}
