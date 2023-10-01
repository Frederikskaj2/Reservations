using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class ReimburseHandler
{
    public static EitherAsync<Failure, Unit> Handle(ReimburseCommand command, IPersistenceContextFactory contextFactory) =>
        from context1 in ReadUserContext(CreateContext(contextFactory), command.UserId)
        from context2 in ReimburseFunctions.Reimburse(command, context1)
        from _ in WriteContext(context2)
        // TODO: Consider sending an email to the user.
        select unit;
}
