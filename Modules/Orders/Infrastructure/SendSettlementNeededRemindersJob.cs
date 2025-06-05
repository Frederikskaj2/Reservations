using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;

namespace Frederikskaj2.Reservations.Orders;

class SendSettlementNeededRemindersJob(
    IDateProvider dateProvider,
    IOrdersEmailService emailService,
    IOptionsSnapshot<OrderingOptions> options,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendSettlementNeededRemindersShell.SendSettlementNeededReminders(emailService, options.Value, reader, writer, new(dateProvider.Today), cancellationToken);
}
