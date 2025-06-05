using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails;
using LanguageExt;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class UsersEmailService(
        IOptionsSnapshot<EmailsOptions> emailsOptions,
        IEmailEnqueuer emailEnqueuer,
        IOptionsSnapshot<TokenEncryptionOptions> tokenEncryptionOptions,
        TokenFactory tokenFactory)
    : IUsersEmailService
{
    readonly EmailsOptions emailsOptions = emailsOptions.Value;
    readonly TokenEncryptionOptions tokenEncryptionOptions = tokenEncryptionOptions.Value;

    public async Task<Unit> Send(ConfirmEmailEmailModel model, CancellationToken cancellationToken)
    {
        var (timestamp, userId, emailAddress, fullName) = model;
        var token = tokenFactory.GetConfirmEmailToken(timestamp, userId);
        var url = GetConfirmEmailUrl(emailsOptions.BaseUrl, emailAddress, token);
        var email = new Email(emailAddress, fullName, emailsOptions.BaseUrl) { ConfirmEmail = new(url, tokenEncryptionOptions.ConfirmEmailDuration) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(NewPasswordEmailModel model, CancellationToken cancellationToken)
    {
        var (timestamp, emailAddress, fullName) = model;
        var token = tokenFactory.GetNewPasswordToken(timestamp, emailAddress);
        var url = GetNewPasswordUrl(emailsOptions.BaseUrl, emailAddress, token);
        var email = new Email(emailAddress, fullName, emailsOptions.BaseUrl) { NewPassword = new(url, tokenEncryptionOptions.NewPasswordDuration) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(UserDeletedEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName) = model;
        var email = new Email(emailAddress, fullName, emailsOptions.BaseUrl) { UserDeleted = new() };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    static Uri GetConfirmEmailUrl(Uri baseUrl, EmailAddress email, ImmutableArray<byte> token)
    {
        var uriBuilder = new UriBuilder(baseUrl)
        {
            Path = UrlPath.ConfirmEmail,
            Query = $"?email={Uri.EscapeDataString(email.ToString()!)}&token={Uri.EscapeDataString(Convert.ToBase64String(token.UnsafeNoCopyToArray()))}",
        };
        return uriBuilder.Uri;
    }

    static Uri GetNewPasswordUrl(Uri baseUrl, EmailAddress email, ImmutableArray<byte> token)
    {
        var uriBuilder = new UriBuilder(baseUrl)
        {
            Path = UrlPath.NewPassword,
            Query = $"?email={Uri.EscapeDataString(email.ToString()!)}&token={Uri.EscapeDataString(Convert.ToBase64String(token.UnsafeNoCopyToArray()))}",
        };
        return uriBuilder.Uri;
    }
}
