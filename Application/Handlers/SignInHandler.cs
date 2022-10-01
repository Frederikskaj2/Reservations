using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.SignInFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SignInHandler
{
    public static EitherAsync<Failure<SignInError>, AuthenticatedUser> Handle(
        IPersistenceContextFactory contextFactory, AuthenticationOptions options, IPasswordHasher passwordHasher, SignInCommand command) =>
        from context1 in ReadUserEmailContext(CreateContext(contextFactory), command.Email)
        from context2 in SignInFunctions.ReadUserContext(context1, context1.Item<UserEmail>().UserId)
        from _1 in CheckLockout(ShouldLockOut(options), command.Timestamp, context2.Item<User>())
        let passwordVerificationResult = VerifyPassword(passwordHasher, context2.Item<User>(), command.Password)
        from context3 in UpdateFailedSignInIfPasswordIsInvalid(command.Timestamp, context2, passwordVerificationResult)
        let context4 = RehashPasswordIfNeeded(passwordHasher, command.Password, context3, passwordVerificationResult)
        from deviceId in CreateDeviceIdIfNeeded(contextFactory, command.DeviceId, passwordVerificationResult)
        let context5 = SignUserInIfPasswordIsValid(options, command.Timestamp, command.IsPersistent, deviceId, passwordVerificationResult, context4)
        from _2 in Write(context5)
        from _3 in HandleInvalidPassword(passwordVerificationResult)
        let user = context5.Item<User>()
        select CreateAuthenticatedUser(user.LatestSignIn!.Value, user, user.Security.RefreshTokens[^1]);
}
