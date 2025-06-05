using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.SendLockBoxCodesShell;

namespace Frederikskaj2.Reservations.Orders;

class SendLockBoxCodesJob(
    IDateProvider dateProvider,
    IOrdersEmailService emailService,
    IOptionsSnapshot<OrderingOptions> options,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendLockBoxCodes(emailService, options.Value, reader, writer, new(dateProvider.Today), cancellationToken);
}
