using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class ConfirmEmailHandler
{
    public static EitherAsync<Failure<ConfirmEmailError>, Unit> Handle(
        IPersistenceContextFactory contextFactory, ITokenProvider tokenProvider, ConfirmEmailCommand command) =>
        from userEmail in ReadUserEmailHideNotFoundStatus(contextFactory, command.Email)
        from _1 in ParseToken(tokenProvider, command.Timestamp, command.Token, userEmail.UserId)
        from context1 in ReadUserContext(contextFactory, userEmail.UserId)
        let context2 = context1.UpdateItem<User>(User.GetId(userEmail.UserId), u => ConfirmUserEmail(u, command.Email, command.Timestamp))
        from _2 in WriteContext(context2)
        select unit;
}
