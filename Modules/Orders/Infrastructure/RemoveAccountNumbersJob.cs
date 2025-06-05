using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.RemoveAccountNumbersShell;

namespace Frederikskaj2.Reservations.Orders;

class RemoveAccountNumbersJob(
    IDateProvider dateProvider,
    IEntityReader entityReader,
    IEntityWriter entityWriter,
    IOptionsSnapshot<OrderingOptions> options)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        RemoveAccountNumbers(options.Value, entityReader, entityWriter, new(dateProvider.Now, UserId.System), cancellationToken);
}
