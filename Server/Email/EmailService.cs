using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailService
    {
        private readonly IClock clock;
        private readonly EmailOptions options;
        private readonly UrlService urlService;
        private readonly RazorViewToStringRenderer viewToStringRenderer;

        public EmailService(
            IOptions<EmailOptions> options, IClock clock, RazorViewToStringRenderer viewToStringRenderer,
            UrlService urlService)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.viewToStringRenderer = viewToStringRenderer ?? throw new ArgumentNullException(nameof(viewToStringRenderer));
            this.urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));
            
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

            using var client = new SmtpClient();
            await client.ConnectAsync(options.SmtpHostName, options.SmtpPort, options.SocketOptions);
            await client.AuthenticateAsync(options.UserName, options.Password);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.FromName, options.FromEmail));
            message.To.Add(new MailboxAddress(user.FullName, user.Email));
            var model = new ConfirmEmailModel(user, urlService.GetConfirmEmailUrl(user.Email, token), options.FromName!, urlService.GetFromUrl());
            message.Subject = await viewToStringRenderer.RenderViewToStringAsync(@"ConfirmEmail\Subject", model);
            var bodyBuilder = new BodyBuilder
            {
                TextBody = await viewToStringRenderer.RenderViewToStringAsync(@"ConfirmEmail\Text", model),
                HtmlBody = await viewToStringRenderer.RenderViewToStringAsync(@"ConfirmEmail\Html", model)
            };
            message.Body = bodyBuilder.ToMessageBody();

            await client.SendAsync(message);
        }
    }
}