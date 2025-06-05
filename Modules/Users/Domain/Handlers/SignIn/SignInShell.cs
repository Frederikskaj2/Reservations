using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Users.SignIn;
using static LanguageExt.Prelude;
using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Users;

public static class SignInShell
{
    public static EitherAsync<Failure<SignInError>, AuthenticatedUser> SignIn(
        AuthenticationOptions options,
        IPasswordHasher passwordHasher,
        IEntityReader reader,
        IEntityWriter writer,
        SignInCommand command,
        CancellationToken cancellationToken) =>
        from userEmail in reader.Read<UserEmail>(command.Email, cancellationToken).MapLeft(MapReadStatus)
        from userEntity in reader.ReadWithETag<User>(userEmail.UserId, cancellationToken).MapLeft(MapReadStatus)
        from _1 in CheckLockout(options, command.Timestamp, userEntity.Value)
        from deviceId in GetProvidedOrCreateNewDeviceId(reader, writer, command.DeviceId, cancellationToken)
        let output = SignInCore(options, passwordHasher, new(command, userEntity.Value, deviceId))
        from _2 in writer.Write(collector => collector.Add(userEntity), tracker => tracker.Update(output.User), cancellationToken)
            .Map(_ => unit)
            .MapWriteError<SignInError>()
        from _3 in HandleInvalidPassword(output.PasswordVerificationResult)
        select CreateAuthenticatedUser(output.User);

    static Failure<SignInError> MapReadStatus(HttpStatusCode status) =>
        status switch
        {
            HttpStatusCode.NotFound => Failure.New(HttpStatusCode.Forbidden, SignInError.InvalidEmailOrPassword),
            _ => Failure.New(HttpStatusCode.ServiceUnavailable, SignInError.Unknown, $"Database read error {status}."),
        };

    static EitherAsync<Failure<SignInError>, Unit> CheckLockout(AuthenticationOptions options, Instant timestamp, User user) =>
        user.FailedSign.Case switch
        {
            FailedSignIn failedSignIn when ShouldLockOut(options, failedSignIn.Count, timestamp - failedSignIn.Timestamp) =>
                Failure.New(HttpStatusCode.Forbidden, SignInError.LockedOut),
            _ => unit,
        };

    static bool ShouldLockOut(AuthenticationOptions options, int failedSignInCount, Duration elapsedSinceLatestAttempt) =>
        failedSignInCount >= options.MaximumAllowedFailedSignInAttempts && elapsedSinceLatestAttempt < options.LockoutPeriod;

    static EitherAsync<Failure<SignInError>, DeviceId> GetProvidedOrCreateNewDeviceId(
        IEntityReader reader, IEntityWriter writer, Option<DeviceId> providedDeviceId, CancellationToken cancellationToken) =>
        providedDeviceId.Case switch
        {
            DeviceId deviceId => deviceId,
            _ => CreateDeviceId(reader, writer, cancellationToken),
        };

    static EitherAsync<Failure<SignInError>, DeviceId> CreateDeviceId(IEntityReader reader, IEntityWriter writer, CancellationToken cancellationToken) =>
        CreateId(reader, writer, nameof(DeviceId), cancellationToken)
            .BiMap(DeviceId.FromInt32, _ => Failure.New(HttpStatusCode.ServiceUnavailable, SignInError.Unknown));

    static EitherAsync<Failure<SignInError>, Unit> HandleInvalidPassword(PasswordVerificationResult passwordVerificationResult) =>
        passwordVerificationResult is PasswordVerificationResult.Failed
            ? Failure.New(HttpStatusCode.Forbidden, SignInError.InvalidEmailOrPassword)
            : unit;

    static AuthenticatedUser CreateAuthenticatedUser(User user) =>
        Authentication.CreateAuthenticatedUser(user.LatestSignIn.IfNone(() => default), user, user.Security.RefreshTokens[^1]);
}
