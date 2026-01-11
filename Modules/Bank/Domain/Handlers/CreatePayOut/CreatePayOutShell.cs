using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Bank.CreatePayOut;
using static Frederikskaj2.Reservations.Bank.PayOutFunctions;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

public static class CreatePayOutShell
{
    public static EitherAsync<Failure<Unit>, PayOutSummary> CreatePayOut(
        IEntityReader reader, IEntityWriter writer, CreatePayOutCommand command, CancellationToken cancellationToken) =>
        from residentExcerpt in ReadUserExcerpt(reader, command.ResidentId, cancellationToken)
        from id in CreateId(reader, writer, nameof(PayOut), cancellationToken)
        let output = CreatePayOutCore(new(command, id))
        from _ in WriteWithConflictHandling(writer, output, cancellationToken)
        select CreatePayOutSummary(output.PayOut, residentExcerpt, None);

    static EitherAsync<Failure<Unit>, Unit> WriteWithConflictHandling(IEntityWriter writer, CreatePayOutOutput output, CancellationToken cancellationToken) =>
        writer
            .Write(tracker => tracker.Add(output.InProgressPayOut).Add(output.PayOut), cancellationToken)
            .BiMap(
                _ => unit,
                status => status switch
                {
                    HttpStatusCode.Conflict => Failure.New(status, "Cannot create more than one in-progress pay-out per resident."),
                    _ => Failure.New(HttpStatusCode.ServiceUnavailable, $"Database write error {status}."),
                }
            );
}
