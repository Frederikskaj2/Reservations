using Frederikskaj2.Reservations.Shared.Email;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.UrlFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

class EmailService : IEmailService
{
    readonly ApplicationOptions applicationOptions;
    readonly IEmailQueue emailQueue;
    readonly TokenEncryptionOptions tokenEncryptionOptions;
    readonly ITokenProvider tokenProvider;

    public EmailService(
        IEmailQueue emailQueue, IOptionsSnapshot<ApplicationOptions> applicationOptions, IOptionsSnapshot<TokenEncryptionOptions> tokenEncryptionOptions,
        ITokenProvider tokenProvider)
    {
        this.emailQueue = emailQueue;
        this.applicationOptions = applicationOptions.Value;
        this.tokenEncryptionOptions = tokenEncryptionOptions.Value;
        this.tokenProvider = tokenProvider;
    }

    public async Task<Unit> Send(CleaningScheduleEmail model, IEnumerable<EmailUser> users)
    {
        var (schedule, delta) = model;
        foreach (var user in users)
        {
            var email = new Email(applicationOptions.BaseUrl) { CleaningSchedule = new(user.Email, user.FullName, schedule, delta, Resources.GetAll().Values) };
            await emailQueue.Enqueue(email);
        }

        return unit;
    }

    public async Task<Unit> Send(ConfirmEmailEmailModel model)
    {
        var (timestamp, userId, emailAddress, fullName) = model;
        var token = tokenProvider.GetConfirmEmailToken(timestamp, userId);
        var url = GetConfirmEmailUrl(applicationOptions.BaseUrl, emailAddress, token);
        var email = new Email(applicationOptions.BaseUrl) { ConfirmEmail = new(emailAddress, fullName, url, tokenEncryptionOptions.ConfirmEmailDuration) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(DebtReminderEmailModel model)
    {
        var (emailAddress, fullName, paymentInformation) = model;
        var email = new Email(applicationOptions.BaseUrl) { DebtReminder = new(emailAddress, fullName, paymentInformation) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(LockBoxCodesEmail model)
    {
        var (emailAddress, fullName, orderId, resourceId, date, codes) = model;
        var email = new Email(applicationOptions.BaseUrl)
        {
            LockBoxCodes = new(
                emailAddress,
                fullName,
                orderId,
                GetMyOrderUrl(applicationOptions.BaseUrl, orderId),
                Resources.Name(resourceId),
                GetRulesUrl(applicationOptions.BaseUrl, resourceId),
                date,
                codes)
        };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(LockBoxCodesOverviewEmail model)
    {
        var (emailAddress, fullName, codes) = model;
        var email = new Email(applicationOptions.BaseUrl) { LockBoxCodesOverview = new(emailAddress, fullName, Resources.GetAll().Values, codes) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(NewOrderEmail model, IEnumerable<EmailUser> users)
    {
        var orderId = model.OrderId;
        var orderUrl = GetOrderUrl(applicationOptions.BaseUrl, orderId);
        foreach (var user in users)
        {
            var email = new Email(applicationOptions.BaseUrl) { NewOrder = new(user.Email, user.FullName, orderId, orderUrl) };
            await emailQueue.Enqueue(email);
        }
        return unit;
    }

    public async Task<Unit> Send(NewPasswordEmailModel model)
    {
        var (timestamp, emailAddress, fullName) = model;
        var token = tokenProvider.GetNewPasswordToken(timestamp, emailAddress);
        var url = GetNewPasswordUrl(applicationOptions.BaseUrl, emailAddress, token);
        var email = new Email(applicationOptions.BaseUrl) { NewPassword = new(emailAddress, fullName, url, tokenEncryptionOptions.NewPasswordDuration) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(NoFeeCancellationAllowedModel model)
    {
        var (emailAddress, fullName, orderId, duration) = model;
        var orderUrl = GetMyOrderUrl(applicationOptions.BaseUrl, orderId);
        var email = new Email(applicationOptions.BaseUrl) { NoFeeCancellationAllowed = new(emailAddress, fullName, orderId, orderUrl, duration) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(OrderConfirmedEmailModel model)
    {
        var (emailAddress, fullName, orderId) = model;
        var orderUrl = GetMyOrderUrl(applicationOptions.BaseUrl, orderId);
        var email = new Email(applicationOptions.BaseUrl) { OrderConfirmed = new(emailAddress, fullName, orderId, orderUrl) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(OrderReceivedEmailModel model)
    {
        var (emailAddress, fullName, orderId, paymentInformation) = model;
        var orderUrl = GetMyOrderUrl(applicationOptions.BaseUrl, orderId);
        var email = new Email(applicationOptions.BaseUrl) { OrderReceived = new(emailAddress, fullName, orderId, orderUrl, paymentInformation) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(PayInEmailModel model)
    {
        var (emailAddress, fullName, amount, payment) = model;
        var email = new Email(applicationOptions.BaseUrl) { PayIn = new(emailAddress, fullName, amount, payment) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(PayOutEmailModel model)
    {
        var (emailAddress, fullName, amount) = model;
        var email = new Email(applicationOptions.BaseUrl) { PayOut = new(emailAddress, fullName, amount) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(PostingsForMonthEmailModel model)
    {
        var (emailAddress, fullName, month, postings) = model;
        var email = new Email(applicationOptions.BaseUrl) { PostingsForMonth = new(emailAddress, fullName, month, AccountNames.GetAll(), postings) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(ReservationsCancelledEmailModel model)
    {
        var (emailAddress, fullName, orderId, reservations, refund, fee) = model;
        var orderUrl = GetMyOrderUrl(applicationOptions.BaseUrl, orderId);
        var email = new Email(applicationOptions.BaseUrl) { ReservationsCancelled = new(emailAddress, fullName, orderId, orderUrl, reservations, refund, fee) };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(ReservationSettledEmailModel model)
    {
        var (emailAddress, fullName, orderId, reservation, deposit, damages, description) = model;
        var orderUrl = GetMyOrderUrl(applicationOptions.BaseUrl, orderId);
        var email = new Email(applicationOptions.BaseUrl)
        {
            ReservationSettled = new(emailAddress, fullName, orderId, orderUrl, reservation, deposit, damages, description)
        };
        await emailQueue.Enqueue(email);
        return unit;
    }

    public async Task<Unit> Send(SettlementNeededEmail model, IEnumerable<EmailUser> users)
    {
        var (orderId, resourceId, date) = model;
        var orderUrl = GetOrderUrl(applicationOptions.BaseUrl, orderId);
        var resourceName = Resources.Name(resourceId);
        foreach (var user in users)
        {
            var email = new Email(applicationOptions.BaseUrl) { SettlementNeeded = new(user.Email, user.FullName, orderId, orderUrl, resourceName, date) };
            await emailQueue.Enqueue(email);
        }
        return unit;
    }

    public async Task<Unit> Send(UserDeletedEmailModel model)
    {
        var (emailAddress, fullName) = model;
        var email = new Email(applicationOptions.BaseUrl) { UserDeleted = new(emailAddress, fullName) };
        await emailQueue.Enqueue(email);
        return unit;
    }
}
