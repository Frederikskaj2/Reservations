using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.UpdatePasswordFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class NewPasswordHandler
{
    public static EitherAsync<Failure<NewPasswordError>, Unit> Handle(
        IPersistenceContextFactory contextFactory, IPasswordHasher passwordHasher, IPasswordValidator passwordValidator, ITokenProvider tokenProvider,
        NewPasswordCommand command) =>
        from userEmail in ReadUserEmailNewPassword(contextFactory, command.Email)
        from _1 in ParseToken(tokenProvider, command.Timestamp, command.Token, command.Email)
        from context1 in ReadUserContextNewPassword(CreateContext(contextFactory), userEmail.UserId)
        from _2 in ValidateNewPassword(passwordValidator, command.Email, command.NewPassword)
        let hashedPassword = passwordHasher.HashPassword(command.NewPassword)
        let context2 = context1.UpdateItem<User>(u => UpdateUser(command.Timestamp, u, hashedPassword))
        from _3 in Write<NewPasswordError>(context2)
        select unit;
}
