using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.ErrorHandling;
using Frederikskaj2.Reservations.Shared;
using MailKit.Net.Smtp;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using LockBoxCode = Frederikskaj2.Reservations.Server.Data.LockBoxCode;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger logger;
        private readonly EmailOptions options;
        private readonly ReservationsOptions reservationsOptions;
        private readonly UrlService urlService;
        private readonly RazorViewToStringRenderer viewToStringRenderer;

        public EmailService(
            ILogger<EmailService> logger, IOptions<EmailOptions> options,
            RazorViewToStringRenderer viewToStringRenderer, UrlService urlService,
            ReservationsOptions reservationsOptions)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.viewToStringRenderer =
                viewToStringRenderer ?? throw new ArgumentNullException(nameof(viewToStringRenderer));
            this.urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));

            this.options = options.Value;
            if (string.IsNullOrEmpty(this.options.SmtpHostName))
                throw new ConfigurationException("Missing SMTP host name.");
            if (string.IsNullOrEmpty(this.options.UserName))
                throw new ConfigurationException("Missing user name.");
            if (string.IsNullOrEmpty(this.options.Password))
                throw new ConfigurationException("Missing password.");
            if (string.IsNullOrEmpty(this.options.From?.Name))
                throw new ConfigurationException("Missing from name.");
            if (string.IsNullOrEmpty(this.options.From?.Email))
                throw new ConfigurationException("Missing from email.");
            if (this.options.BaseUrl == null)
                throw new ConfigurationException("Missing base URL.");
            if (this.options.ConfirmEmailUrlLifetime <= Duration.Zero)
                throw new ConfigurationException("Invalid confirm email url lifetime.");
        }

        public async Task SendConfirmEmail(User user, string token)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Value cannot be null or empty.", nameof(token));

            var model = new EmailWithUrlModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetConfirmEmailUrl(user.Email, token));
            await SendMessage(model, "ConfirmEmail", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendResetPasswordEmail(User user, string token)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Value cannot be null or empty.", nameof(token));

            var model = new EmailWithUrlModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetResetPasswordUrl(user.Email, token));
            await SendMessage(model, "ResetPassword", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendOrderReceivedEmail(User user, int orderId, int prepaidAmount, int amount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (prepaidAmount < 0 || prepaidAmount > amount)
                throw new ArgumentOutOfRangeException(nameof(prepaidAmount));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var model = new OrderReceivedModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(orderId),
                orderId,
                prepaidAmount,
                amount,
                reservationsOptions.PayInAccountNumber);
            await SendMessage(model, "OrderReceived", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendNewOrderEmail(User user, int orderId)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var model = new OrderModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId);
            await SendMessage(model, "NewOrder", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendOrderConfirmedEmail(User user, int orderId)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var model = new OrderModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(orderId),
                orderId);
            await SendMessage(model, "OrderConfirmed", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendPayInEmail(User user, int orderId, int amount, int missingAmount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (missingAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(missingAmount));

            var model = new PayInModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(orderId),
                orderId,
                amount,
                missingAmount,
                reservationsOptions.PayInAccountNumber);
            await SendMessage(model, "PayIn", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendOverduePaymentEmail(User user, int orderId)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var model = new OrderModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId);
            await SendMessage(model, "OverduePayment", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendReservationCancelledEmail(
            User user, int orderId, string resourceName, LocalDate date, int total, int cancellationFee)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));
            if (total < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));
            if (cancellationFee < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            var model = new ReservationCancelledModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(orderId),
                orderId,
                resourceName,
                date,
                total,
                cancellationFee);
            await SendMessage(
                model, "ReservationCancelled", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendSettlementNeededEmail(User user, int orderId, string resourceName, LocalDate date)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));

            var model = new ReservationModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                resourceName,
                date);
            await SendMessage(model, "SettlementNeeded", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendReservationSettledEmail(
            User user, int orderId, string resourceName, LocalDate date, int deposit, int damages,
            string? damagesDescription)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));
            if (deposit <= 0)
                throw new ArgumentOutOfRangeException(nameof(deposit));
            if (damages < 0)
                throw new ArgumentOutOfRangeException(nameof(damages));
            if (damages > 0 && string.IsNullOrEmpty(damagesDescription))
                throw new ArgumentException("Value cannot be null or empty.", nameof(damagesDescription));

            var model = new ReservationSettledModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(orderId),
                orderId,
                resourceName,
                date,
                deposit,
                damages,
                damagesDescription);
            await SendMessage(
                model, "ReservationSettled", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendPayOutEmail(User user, int amount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var model = new PayOutModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                amount);
            await SendMessage(model, "PayOut", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendUserDeletedEmail(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var model = new EmailWithNameModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName);
            await SendMessage(model, "UserDeleted", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendLockBoxCodeEmail(User user, Reservation reservation, IEnumerable<LockBoxCode> lockBoxCodes)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));
            if (lockBoxCodes is null)
                throw new ArgumentNullException(nameof(lockBoxCodes));

            var model = new LockBoxCodeModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetMyOrderUrl(reservation.OrderId),
                reservation.OrderId,
                reservation.Resource!.Name,
                reservation.Date,
                GetDatedLockBoxCodes().ToList(),
                urlService.GetRulesUri(reservation.Resource.Type));
            await SendMessage(model, "LockBoxCode", new EmailRecipient { Name = user.FullName, Email = user.Email });

            IEnumerable<DatedLockBoxCode> GetDatedLockBoxCodes()
            {
                var previousMonday = GetPreviousMonday(reservation.Date);
                var firstCode = lockBoxCodes.FirstOrDefault(
                    code => code.ResourceId == reservation.ResourceId && code.Date == previousMonday);
                if (firstCode == null)
                    yield break;
                yield return new DatedLockBoxCode(reservation.Date, firstCode.Code);
                var nextMonday = previousMonday.PlusWeeks(1);
                if (reservation.Date.PlusDays(reservation.DurationInDays) < nextMonday)
                    yield break;
                var nextCode = lockBoxCodes.FirstOrDefault(
                    code => code.ResourceId == reservation.ResourceId && code.Date == nextMonday);
                if (nextCode == null)
                    yield break;
                yield return new DatedLockBoxCode(nextMonday, nextCode.Code);
            }
        }

        public async Task SendPostingsEmail(User user, IEnumerable<Shared.Posting> postings)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (postings is null)
                throw new ArgumentNullException(nameof(postings));

            var model = new PostingsModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName!,
                postings.AsQueryable().ProjectToType<Posting>());
            await SendMessage(model, "Postings", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        public async Task SendCleaningScheduleEmail(
            User user, IEnumerable<Data.Resource> resources, IEnumerable<Data.CleaningTask> cancelledTasks,
            IEnumerable<Data.CleaningTask> newTasks, IEnumerable<Data.CleaningTask> currentTasks)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));
            if (cancelledTasks is null)
                throw new ArgumentNullException(nameof(cancelledTasks));
            if (newTasks is null)
                throw new ArgumentNullException(nameof(newTasks));
            if (currentTasks is null)
                throw new ArgumentNullException(nameof(currentTasks));

            var resourceDictionary = resources.ToDictionary(resource => resource.Id);
            var cancelledCleaningTasks = GetCleaningTasks(cancelledTasks).ToList();
            var newCleaningTasks = GetCleaningTasks(newTasks).ToList();
            var currentCleaningTasks = GetCleaningTasks(currentTasks).ToList();
            var model = new CleaningScheduleModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName!,
                cancelledCleaningTasks,
                newCleaningTasks,
                currentCleaningTasks);
            await SendMessage(model, "CleaningSchedule", new EmailRecipient { Name = user.FullName, Email = user.Email });

            IEnumerable<CleaningTask> GetCleaningTasks(IEnumerable<Data.CleaningTask> tasks)
                => tasks.Select(
                        task => new CleaningTask(
                            task.Date,
                            resourceDictionary[task.ResourceId].Name,
                            resourceDictionary[task.ResourceId].Sequence))
                    .OrderBy(task => task.Date)
                    .ThenBy(task => task.ResourceName);
        }

        public async Task SendWeeklyLockBoxCodesEmail(
            User user, IEnumerable<Data.Resource> resources, IEnumerable<WeeklyLockBoxCodes> weeklyLockBoxCodes)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));
            if (weeklyLockBoxCodes is null)
                throw new ArgumentNullException(nameof(weeklyLockBoxCodes));

            var resourcesDictionary = resources.ToDictionary(
                resource => resource.Id,
                resource => new Resource(resource.Id, resource.Sequence, resource.Name));
            var model = new WeeklyLockBoxCodesModel(
                options.From!.Name!,
                urlService.GetFromUrl(),
                user.FullName,
                resourcesDictionary,
                weeklyLockBoxCodes);
            await SendMessage(model, "WeeklyLockBoxCodes", new EmailRecipient { Name = user.FullName, Email = user.Email });
        }

        private async Task SendMessage<TModel>(TModel model, string viewName, EmailRecipient recipient)
        {
            var message = await CreateMessage(model, viewName, recipient);
            await SendMessage(message);
            logger.LogInformation("Sent message {Message} to {Email}", viewName, recipient.Email.MaskEmail());
        }

        private async Task<MimeMessage> CreateMessage<TModel>(
            TModel model, string viewName, EmailRecipient recipient)
        {
            var message = CreateEmptyMessage(recipient);
            message.Subject = await viewToStringRenderer.RenderViewToStringAsync($@"{viewName}\Subject", model);
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = await viewToStringRenderer.RenderViewToStringAsync($@"{viewName}\Html", model)
            };
            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private MimeMessage CreateEmptyMessage(EmailRecipient recipient)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.From!.Name, options.From.Email));
            message.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
            if (!string.IsNullOrEmpty(options.ReplyTo?.Name) && !string.IsNullOrEmpty(options.ReplyTo?.Email))
                message.ReplyTo.Add(new MailboxAddress(options.ReplyTo.Name, options.ReplyTo.Email));
            return message;
        }

        private async Task SendMessage(MimeMessage message)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(options.SmtpHostName, options.SmtpPort, options.SocketOptions);
            await client.AuthenticateAsync(options.UserName, options.Password);
            await client.SendAsync(message);
        }

        private static LocalDate GetPreviousMonday(LocalDate date)
        {
            var daysAfterMonday = ((int) date.DayOfWeek - 1)%7;
            return date.PlusDays(-daysAfterMonday);
        }
    }
}