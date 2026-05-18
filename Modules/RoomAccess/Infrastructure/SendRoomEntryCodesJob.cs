using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.RoomAccess.SendRoomEntryCodesShell;

namespace Frederikskaj2.Reservations.RoomAccess;

class SendRoomEntryCodesJob(
    IDateProvider dateProvider,
    IRoomAccessEmailService emailService,
    IOptionsSnapshot<OrderingOptions> options,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendRoomEntryCodes(emailService, options.Value, reader, writer, new(dateProvider.Today), cancellationToken);
}
