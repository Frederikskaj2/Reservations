using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Threading;
using static Frederikskaj2.Reservations.Emails.SendEmailsShell;

namespace Frederikskaj2.Reservations.Emails;

class SendEmailsJob(IEmailDequeuer emailDequeuer, EmailSender emailSender, MessageFactory messageFactory) : IJob
{
    public EitherAsync<Failure<Unit>, Unit> Invoke(CancellationToken cancellationToken) =>
        SendEmails(emailDequeuer, emailSender, messageFactory, cancellationToken);
}
