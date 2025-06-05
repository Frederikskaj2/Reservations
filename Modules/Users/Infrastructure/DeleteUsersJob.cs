using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Users.DeleteUsersShell;

namespace Frederikskaj2.Reservations.Users;

class DeleteUsersJob(
    IDateProvider dateProvider,
    IUsersEmailService emailService,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        DeleteUsers(emailService, reader, writer, new(dateProvider.Now), cancellationToken);
}
