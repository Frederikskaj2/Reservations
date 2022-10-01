using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PostingFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class SendPostingsHandler
{
    public static EitherAsync<Failure, EmailAddress> Handle(
        IPersistenceContextFactory contextFactory, IEmailService emailService, SendPostingsCommand command) =>
        SendPostingsForMonth(CreateContext(contextFactory), emailService, command);
}
