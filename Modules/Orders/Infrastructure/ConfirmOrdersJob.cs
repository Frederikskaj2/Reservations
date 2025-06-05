using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.ConfirmOrdersShell;

namespace Frederikskaj2.Reservations.Orders;

class ConfirmOrdersJob(
    IDateProvider dateProvider,
    IOrdersEmailService emailService,
    IJobScheduler jobScheduler,
    IEntityReader reader,
    IEntityWriter writer)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        ConfirmOrders(emailService, jobScheduler, reader, writer, new(dateProvider.Now), cancellationToken);
}
