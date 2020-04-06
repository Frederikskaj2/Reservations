using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailService
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
            if (string.IsNullOrEmpty(this.options.FromName))
                throw new ConfigurationException("Missing from name.");
            if (string.IsNullOrEmpty(this.options.FromEmail))
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
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetConfirmEmailUrl(user.Email, token));
            await SendMessage(user, model, "COnfirmEmail");
        }

        public async Task SendResetPasswordEmail(User user, string token)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Value cannot be null or empty.", nameof(token));

            var model = new EmailWithUrlModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetResetPasswordUrl(user.Email, token));
            await SendMessage(user, model, "ResetPassword");
        }

        public async Task SendOrderReceivedEmail(User user, int orderId, int amount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var model = new OrderReceivedModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                amount,
                reservationsOptions.PayInAccountNumber);
            await SendMessage(user, model, "OrderReceived");
        }

        public async Task SendOrderConfirmedEmail(User user, int orderId, int amount, int excessAmount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (excessAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(excessAmount));

            var model = new OrderConfirmedModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                amount,
                excessAmount);
            await SendMessage(user, model, "OrderConfirmed");
        }

        public async Task SendMissingPaymentEmail(User user, int orderId, int amount, int missingAmount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (missingAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(missingAmount));

            var model = new MissingPaymentModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                amount,
                missingAmount,
                reservationsOptions.PayInAccountNumber);
            await SendMessage(user, model, "MissingPayment");
        }

        public async Task SendReservationCancelledEmail(
            User user, int orderId, string resourceName, LocalDate date, int cancellationFee)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));
            if (cancellationFee <= 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            var model = new ReservationCancelledModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                resourceName,
                date,
                cancellationFee);
            await SendMessage(user, model, "ReservationCancelled");
        }

        public async Task SendReservationSettledEmail(
            User user, int orderId, string resourceName, LocalDate date, int deposit, int damages, string? damagesDescription)
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
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                resourceName,
                date,
                deposit,
                damages,
                damagesDescription);
            await SendMessage(user, model, "ReservationSettled");
        }

        public async Task SendPayOutEmail(User user, int orderId, int amount)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var model = new PayOutModel(
                options.FromName!,
                urlService.GetFromUrl(),
                user.FullName,
                urlService.GetOrderUrl(orderId),
                orderId,
                amount);
            await SendMessage(user, model, "PayOut");
        }

        private async Task SendMessage<TModel>(User user, TModel model, string viewName)
        {
            var message = await CreateMessage(user, model, viewName);
            await SendMessage(message);
            logger.LogInformation("Sent message {Message} to {User}", viewName, user.Email);
        }

        private async Task<MimeMessage> CreateMessage<TModel>(User user, TModel model, string viewName)
        {
            var message = CreateEmptyMessage(user);
            message.Subject = await viewToStringRenderer.RenderViewToStringAsync($@"{viewName}\Subject", model);
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = await viewToStringRenderer.RenderViewToStringAsync($@"{viewName}\Html", model)
            };
            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private MimeMessage CreateEmptyMessage(User user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.FromName, options.FromEmail));
            message.To.Add(new MailboxAddress(user.FullName, user.Email));
            return message;
        }

        private async Task SendMessage(MimeMessage message)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(options.SmtpHostName, options.SmtpPort, options.SocketOptions);
            await client.AuthenticateAsync(options.UserName, options.Password);
            await client.SendAsync(message);
        }
    }
}