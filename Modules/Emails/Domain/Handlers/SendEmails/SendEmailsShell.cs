using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Emails.SendEmails;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Emails;

public static class SendEmailsShell
{
    public static EitherAsync<Failure<Unit>, Unit> SendEmails(
        IEmailDequeuer emailDequeuer, EmailSender emailSender, MessageFactory messageFactory, CancellationToken cancellationToken)
    {
        return Run().ToAsync();

        async Task<Either<Failure<Unit>, Unit>> Run()
        {
            var outputs = emailDequeuer.Dequeue(cancellationToken).SelectAwait(email => SendEmailCore(messageFactory, new(email), cancellationToken));
            await foreach (var output in outputs.WithCancellation(cancellationToken))
                await emailSender.Send(output.Message, cancellationToken);
            return unit;
        }
    }
}
