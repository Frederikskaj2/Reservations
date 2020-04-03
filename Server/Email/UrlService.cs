using System;
using Frederikskaj2.Reservations.Shared;
using Microsoft.Extensions.Options;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class UrlService
    {
        private readonly EmailOptions options;

        public UrlService(IOptions<EmailOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.options = options.Value;
        }

        public Uri GetFromUrl() => options.BaseUrl!;

        public Uri GetConfirmEmailUrl(string email, string token)
        {
            var uriBuilder = new UriBuilder(options.BaseUrl!)
            {
                Path = Urls.ConfirmEmail,
                Query = $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}"
            };
            return uriBuilder.Uri;
        }

        public Uri GetResetPasswordUrl(string email, string token)
        {
            var uriBuilder = new UriBuilder(options.BaseUrl!)
            {
                Path = Urls.NewPassword,
                Query = $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}"
            };
            return uriBuilder.Uri;
        }
    }
}