using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.UpdatePasswordFunctions;
using static Frederikskaj2.Reservations.Application.UserFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class UpdatePasswordHandler
{
    public static EitherAsync<Failure<PasswordError>, AuthenticatedUser> Handle(
        IPersistenceContextFactory contextFactory, AuthenticationOptions options,
        IPasswordHasher passwordHasher, IPasswordValidator passwordValidator,
        UpdatePasswordCommand command) =>
        from context1 in ReadUserContextUpdatePassword(DatabaseFunctions.CreateContext(contextFactory), command.ParsedToken.UserId)
        let user = context1.Item<User>()
        from existingRefreshToken in GetExistingRefreshToken(command.ParsedToken.TokenId, user)
        from _1 in ValidateUpdatePassword(passwordValidator, user.Email(), command.NewPassword)
        from _2 in VerifyCurrentPassword(passwordHasher, user, command.CurrentPassword)
        let hashedPassword = passwordHasher.HashPassword(command.NewPassword)
        let newRefreshToken = CreateNewRefreshToken(options, command.Timestamp, user, existingRefreshToken)
        let context2 = context1.UpdateItem<User>(u => UpdateUser(command.Timestamp, u, hashedPassword, newRefreshToken))
        from _3 in Write<PasswordError>(context2)
        select CreateAuthenticatedUser(command.Timestamp, context2.Item<User>(), newRefreshToken);
}
