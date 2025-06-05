using System;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Emails.CleaningScheduleFunctions;

namespace Frederikskaj2.Reservations.Emails;

static class SendEmails
{
    public static async ValueTask<SendEmailsOutput> SendEmailCore(MessageFactory messageFactory, SendEmailsInput input, CancellationToken cancellationToken) =>
        new(await CreateMessage(messageFactory, input.Email, cancellationToken));

    static async ValueTask<EmailMessage> CreateMessage(MessageFactory messageFactory, Email email, CancellationToken cancellationToken) =>
        email switch
        {
            { CleaningScheduleOverview: not null } =>
                await messageFactory.CreateMessage(
                    email.ToEmail, email.ToFullName, email.FromUrl, await CreateCleaningCalendar(email.CleaningScheduleOverview, cancellationToken)),
            { ConfirmEmail: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.ConfirmEmail),
            { DebtReminder: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.DebtReminder),
            { LockBoxCodes: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.LockBoxCodes),
            { LockBoxCodesOverview: not null } =>
                await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.LockBoxCodesOverview),
            { NewOrder: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.NewOrder),
            { NewPassword: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.NewPassword),
            { NoFeeCancellationAllowed: not null } =>
                await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.NoFeeCancellationAllowed),
            { OrderConfirmed: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.OrderConfirmed),
            { OrderReceived: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.OrderReceived),
            { PayIn: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.PayIn),
            { PayOut: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.PayOut),
            { PostingsForMonth: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.PostingsForMonth),
            { ReservationsCancelled: not null } =>
                await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.ReservationsCancelled),
            { ReservationSettled: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.ReservationSettled),
            { SettlementNeeded: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.SettlementNeeded),
            { UserDeleted: not null } => await messageFactory.CreateMessage(email.ToEmail, email.ToFullName, email.FromUrl, email.UserDeleted),
            _ => throw new ArgumentException("Empty or unknown email.", nameof(email)),
        };
}
