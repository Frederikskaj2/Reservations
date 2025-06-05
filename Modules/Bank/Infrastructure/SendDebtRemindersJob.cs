using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;

namespace Frederikskaj2.Reservations.Bank;

class SendDebtRemindersJob(
    IDateProvider dateProvider,
    IBankEmailService emailService,
    IEntityReader entityReader,
    IEntityWriter entityWriter,
    IOptionsSnapshot<OrderingOptions> options)
    : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendDebtRemindersShell.SendDebtReminders(emailService, options.Value, entityReader, entityWriter, new(dateProvider.Now), cancellationToken);
}