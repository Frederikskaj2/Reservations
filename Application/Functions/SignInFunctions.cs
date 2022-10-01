using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System;
using System.Net;
using static LanguageExt.Prelude;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Application;

static class SignInFunctions
{
    public static EitherAsync<Failure<SignInError>, IPersistenceContext> ReadUserEmailContext(IPersistenceContext context, EmailAddress email) =>
        HandleReadError(context.ReadItem<UserEmail>(EmailAddress.NormalizeEmail(email)));

    public static EitherAsync<Failure<SignInError>, IPersistenceContext> ReadUserContext(IPersistenceContext context, UserId userId) =>
        HandleReadError(context.ReadItem<User>(User.GetId(userId)));

    static EitherAsync<Failure<SignInError>, IPersistenceContext> HandleReadError(EitherAsync<HttpStatusCode, IPersistenceContext> either) =>
        either.MapLeft(MapReadError);

    static Failure<SignInError> MapReadError(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => Failure.New(HttpStatusCode.Forbidden, SignInError.InvalidEmailOrPassword),
            _ => Failure.New(HttpStatusCode.ServiceUnavailable, SignInError.Unknown, $"Database read error {status}.")
        };

    public static EitherAsync<Failure<SignInError>, IPersistenceContext> Write(IPersistenceContext context) =>
        HandleWriteError(context.Write());

    static EitherAsync<Failure<SignInError>, IPersistenceContext> HandleWriteError(EitherAsync<HttpStatusCode, IPersistenceContext> either) =>
        either.MapLeft(MapWriteError);

    static Failure<SignInError> MapWriteError(HttpStatusCode status) =>
        Failure.New(HttpStatusCode.ServiceUnavailable, SignInError.Unknown, $"Database write error {status}.");

    public static PasswordVerificationResult VerifyPassword(IPasswordHasher passwordHasher, User user, string password) =>
        passwordHasher.VerifyHashedPassword(user.Security.HashedPassword, password);

    public static EitherAsync<Failure<SignInError>, Unit> CheckLockout(Func<int, Duration, bool> shouldLockOut, Instant timestamp, User user) =>
        shouldLockOut(user.FailedSign?.Count ?? 0, timestamp - (user.FailedSign?.Timestamp).GetValueOrDefault())
            ? Failure.New(HttpStatusCode.Forbidden, SignInError.LockedOut)
            : unit;

    public static EitherAsync<Failure<SignInError>, IPersistenceContext> UpdateFailedSignInIfPasswordIsInvalid(
        Instant timestamp, IPersistenceContext context, PasswordVerificationResult passwordVerificationResult) =>
        RightAsync<Failure<SignInError>, IPersistenceContext>(passwordVerificationResult is PasswordVerificationResult.Failed ? UpdateFailedSign(timestamp, context) : context);

    static IPersistenceContext UpdateFailedSign(Instant timestamp, IPersistenceContext context) =>
        context.UpdateItem<User>(user => user with { FailedSign = new FailedSignIn(timestamp, (user.FailedSign?.Count ?? 0) + 1) });

    public static IPersistenceContext RehashPasswordIfNeeded(
        IPasswordHasher passwordHasher, string password, IPersistenceContext context, PasswordVerificationResult passwordVerificationResult) =>
        passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded ? RehashPassword(passwordHasher, password, context) : context;

    static IPersistenceContext RehashPassword(IPasswordHasher passwordHasher, string password, IPersistenceContext context) =>
        context.UpdateItem<User>(user => user with { Security = user.Security with { HashedPassword = passwordHasher.HashPassword(password) } });

    public static EitherAsync<Failure<SignInError>, DeviceId> CreateDeviceIdIfNeeded(
        IPersistenceContextFactory contextFactory, Option<DeviceId> deviceId, PasswordVerificationResult passwordVerificationResult) =>
        deviceId.Case switch
        {
            DeviceId value => value,
            _ => CreateDeviceId(contextFactory, passwordVerificationResult)
                .Map(DeviceId.FromInt32)
                .MapLeft(_ => Failure.New(HttpStatusCode.ServiceUnavailable, SignInError.Unknown))
        };

    static EitherAsync<Failure, int> CreateDeviceId(IPersistenceContextFactory contextFactory, PasswordVerificationResult passwordVerificationResult) =>
        passwordVerificationResult != PasswordVerificationResult.Failed ? IdGenerator.CreateId(contextFactory, "Device") : 0;

    public static IPersistenceContext SignUserInIfPasswordIsValid(
        AuthenticationOptions options, Instant timestamp, bool isPersistent, DeviceId deviceId, PasswordVerificationResult passwordVerificationResult,
        IPersistenceContext context) =>
        passwordVerificationResult != PasswordVerificationResult.Failed
            ? context.UpdateItem<User>(user => SignUserIn(options, timestamp, isPersistent, deviceId, user))
            : context;

    static User SignUserIn(AuthenticationOptions options, Instant timestamp, bool isPersistent, DeviceId deviceId, User user)
    {
        var token = new RefreshToken(
            user.Security.NextRefreshTokenId,
            Authentication.GetExpireAt(options, timestamp, isPersistent),
            isPersistent,
            deviceId);
        return user with
        {
            LatestSignIn = timestamp,
            Security = user.Security with
            {
                NextRefreshTokenId = token.TokenId.NextId,
                RefreshTokens = GetValidTokens(timestamp, user, deviceId).Add(token)
            }
        };
    }

    static Seq<RefreshToken> GetValidTokens(Instant timestamp, User user, DeviceId deviceId) =>
        // Purge all expired tokens and also all other tokens from same device. That's the purpose of the device ID: To make it possible to only store a single
        // valid token per device.
        user.Security.RefreshTokens.Filter(token => token.DeviceId != deviceId && token.ExpireAt >= timestamp);

    public static EitherAsync<Failure<SignInError>, Unit> HandleInvalidPassword(PasswordVerificationResult passwordVerificationResult) =>
        passwordVerificationResult == PasswordVerificationResult.Failed
            ? Failure.New(HttpStatusCode.Forbidden, SignInError.InvalidEmailOrPassword)
            : unit;

    public static Func<int, Duration, bool> ShouldLockOut(AuthenticationOptions options) => (count, elapsed) =>
        count >= options.MaximumAllowedFailedSignInAttempts && elapsed < options.LockoutPeriod;
}
