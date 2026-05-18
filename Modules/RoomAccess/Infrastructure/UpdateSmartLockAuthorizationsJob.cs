using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.RoomAccess.UpdateSmartLockAuthorizationsShell;

namespace Frederikskaj2.Reservations.RoomAccess;

class UpdateSmartLockAuthorizationsJob(
    IDateProvider dateProvider,
    IOptionsSnapshot<OrderingOptions> options,
    IEntityReader reader,
    ISmartLockService smartLockService,
    ITimeConverter timeConverter)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        UpdateSmartLockAuthorizations(options.Value, reader, smartLockService, timeConverter, new(dateProvider.Today), cancellationToken);
}
