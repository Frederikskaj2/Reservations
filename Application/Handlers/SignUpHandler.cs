using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.SignUpFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class SignUpHandler
{
    public static EitherAsync<Failure<SignUpError>, Unit> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, IPasswordHasher passwordHasher, IPasswordValidator passwordValidator,
        SignUpCommand command) =>
        from _1 in ValidatePassword(passwordValidator, command)
        from userId in CreateId(contextFactory)
        let context = CreateUser(passwordHasher, CreateContext(contextFactory), command, UserId.FromInt32(userId))
        from tuple in Write(context)
        from _3 in SendEmail(emailService, command.Timestamp, tuple.User, tuple.NewOrExisting).ToRightAsync<Failure<SignUpError>, Unit>()
        select unit;
}
