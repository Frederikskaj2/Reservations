using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

static class SignIn
{
    public static SignInOutput SignInCore(AuthenticationOptions options, IPasswordHasher passwordHasher, SignInInput input) =>
        CreateOutput(options, passwordHasher, input, VerifyPassword(passwordHasher, input.User, input.Command.Password));

    static SignInOutput CreateOutput(
        AuthenticationOptions options, IPasswordHasher passwordHasher, SignInInput input, PasswordVerificationResult passwordVerificationResult) =>
        new(UpdateUser(options, passwordHasher, input, passwordVerificationResult), passwordVerificationResult);

    static PasswordVerificationResult VerifyPassword(IPasswordHasher passwordHasher, User user, string password) =>
        passwordHasher.VerifyHashedPassword(user.Security.HashedPassword, password);

    static User UpdateUser(
        AuthenticationOptions options, IPasswordHasher passwordHasher, SignInInput input, PasswordVerificationResult passwordVerificationResult) =>
        SignUserInIfPasswordIsValid(
            options,
            input.Command.Timestamp,
            input.Command.IsPersistent,
            input.DeviceId,
            passwordVerificationResult,
            RehashPasswordIfNeeded(
                passwordHasher,
                input.Command.Password,
                passwordVerificationResult,
                UpdateFailedSignIn(passwordVerificationResult, input.Command.Timestamp, input.User)));

    static User UpdateFailedSignIn(PasswordVerificationResult passwordVerificationResult, Instant timestamp, User user) =>
        passwordVerificationResult is PasswordVerificationResult.Failed ? AddFailedSignIn(timestamp, user) : ClearFailedSign(user);

    static User AddFailedSignIn(Instant timestamp, User user) =>
        user with { FailedSign = new FailedSignIn(timestamp, user.FailedSign.Match(failedSignIn => failedSignIn.Count + 1, 1)) };

    static User ClearFailedSign(User user) =>
        user with { FailedSign = None };

    static User RehashPasswordIfNeeded(
        IPasswordHasher passwordHasher, string password, PasswordVerificationResult passwordVerificationResult, User user) =>
        passwordVerificationResult is PasswordVerificationResult.SuccessRehashNeeded ? RehashPassword(passwordHasher, password, user) : user;

    static User RehashPassword(IPasswordHasher passwordHasher, string password, User user) =>
        user with { Security = user.Security with { HashedPassword = passwordHasher.HashPassword(password) } };

    static User SignUserInIfPasswordIsValid(
        AuthenticationOptions options,
        Instant timestamp,
        bool isPersistent,
        DeviceId deviceId,
        PasswordVerificationResult passwordVerificationResult,
        User user) =>
        passwordVerificationResult != PasswordVerificationResult.Failed
            ? SignUserIn(options, timestamp, isPersistent, deviceId, user)
            : user;

    static User SignUserIn(AuthenticationOptions options, Instant timestamp, bool isPersistent, DeviceId deviceId, User user) =>
        SignUserIn(
            timestamp,
            deviceId,
            user,
            new(user.Security.NextRefreshTokenId, Authentication.GetExpireAt(options, timestamp, isPersistent), isPersistent, deviceId));

    static User SignUserIn(Instant timestamp, DeviceId deviceId, User user, RefreshToken token) =>
        user with
        {
            LatestSignIn = timestamp,
            Security = user.Security with
            {
                NextRefreshTokenId = token.TokenId.NextId, RefreshTokens = GetValidTokens(timestamp, user, deviceId).Add(token),
            },
        };

    static Seq<RefreshToken> GetValidTokens(Instant timestamp, User user, DeviceId deviceId) =>
        // Purge all expired tokens and also all other tokens from the same device.
        // That's the purpose of the device ID: To make it possible to only store a
        // single valid token per device.
        user.Security.RefreshTokens.Filter(token => token.DeviceId != deviceId && token.ExpireAt >= timestamp);
}
