using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails.Messages;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

public class MessageFactory(CultureInfo cultureInfo, ILogger<MessageFactory> logger, IOptionsSnapshot<EmailMessageOptions> options, HtmlRenderer renderer)
{
    [SuppressMessage(
        "Sonar", "S6667:Logging in a catch clause should pass the caught exception as a parameter.",
        Justification = "The exception is rethrown and logged elsewhere.")]
    public async ValueTask<EmailMessage> CreateMessage<TModel>(EmailAddress toEmail, string toFullName, Uri fromUrl, TModel model)
    {
        try
        {
            var messageOptions = ValidateOptions(options.Value);
            var emailModel = new EmailModel<TModel>(model, toFullName, messageOptions.From!.Name!, fromUrl, cultureInfo);
            var subject = (await Render(emailModel, TemplateType.Subject)).TrimEnd('\r', '\n');
            var body = await Render(emailModel, TemplateType.Body);
            return new(messageOptions.From!.Email!, messageOptions.ReplyTo?.Email, [toEmail.ToString()], subject, body);
        }
        catch (Exception)
        {
            logger.LogWarning("Cannot create message {Model}", model);
            throw;
        }
    }

    static EmailMessageOptions ValidateOptions(EmailMessageOptions options)
    {
        if (options.From?.Name is null || options.From.Email is null)
            throw new ConfigurationException("Missing or invalid 'from' email recipient.");
        if (options.ReplyTo is not null && (options.ReplyTo.Name is null || options.ReplyTo.Email is null))
            throw new ConfigurationException("Invalid 'reply to' email recipient.");
        return options;
    }

    Task<string> Render<TModel>(EmailModel<TModel> model, TemplateType templateType) =>
        renderer.Dispatcher.InvokeAsync(async () =>
        {
            var componentType = GetComponentType<TModel>(templateType);
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model });
            var rootComponent = await renderer.RenderComponentAsync(componentType, parameters);
            return rootComponent.ToHtmlString();
        });

    static Type GetComponentType<TModel>(TemplateType templateType) =>
        Type.GetType(GetComponentTypeName<TModel>(templateType))!;

    static string GetComponentTypeName<TModel>(TemplateType templateType) =>
        $"Frederikskaj2.Reservations.Emails.Messages.{GetNamespacePartFromModelType(typeof(TModel))}.{templateType}";

    static string GetNamespacePartFromModelType(Type modelType) =>
        modelType.Name.EndsWith("Dto", StringComparison.Ordinal) ? modelType.Name[..^3] : modelType.Name;
}
