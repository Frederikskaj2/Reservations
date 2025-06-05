using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

public sealed class MessageFactoryFixture : IDisposable
{
    readonly HtmlRenderer htmlRenderer;
    readonly MessageFactory messageFactory;

    public MessageFactoryFixture()
    {
        FromEmailAddress = Generate.Email();
        ReplyToEmailAddress = Generate.Email();
        ToEmailAddress = Generate.Email();
        ToFullName = Generate.FullName();
        FromName = Generate.FullName();
        FromUrl = Generate.Url();

        var serviceProvider = Substitute.For<IServiceProvider>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        htmlRenderer = new(serviceProvider, loggerFactory);
        var optionsSnapshot = Substitute.For<IOptionsSnapshot<EmailMessageOptions>>();
        optionsSnapshot.Value.Returns(new EmailMessageOptions
        {
            From = new()
            {
                Name = FromName,
                Email = FromEmailAddress.ToString(),
            },
            ReplyTo = new()
            {
                Name = Generate.FullName(),
                Email = ReplyToEmailAddress.ToString(),
            },
        });
        messageFactory = new(CultureInfo, NullLogger<MessageFactory>.Instance, optionsSnapshot, htmlRenderer);
    }

    public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfo("da-DK");
    public EmailAddress FromEmailAddress { get; }
    public EmailAddress ReplyToEmailAddress { get; }
    public EmailAddress ToEmailAddress { get; }
    public string ToFullName { get; }
    public string FromName { get; }
    public Uri FromUrl { get; }

    public ValueTask<EmailMessage> CreateMessage<TModel>(TModel model) =>
        messageFactory.CreateMessage(ToEmailAddress.ToString(), ToFullName, FromUrl, model);

    public void Dispose() => htmlRenderer.Dispose();
}
