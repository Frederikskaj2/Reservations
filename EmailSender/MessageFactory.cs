using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.EmailSender.CleaningTasksFunctions;

namespace Frederikskaj2.Reservations.EmailSender;

class MessageFactory
{
    static readonly Dictionary<TemplateKey, object> templateCache = new();
    readonly CultureInfo cultureInfo;
    readonly ILogger logger;
    readonly IOptionsMonitor<EmailMessageOptions> optionsMonitor;
    readonly TemplateEngine templateEngine;

    public MessageFactory(
        CultureInfo cultureInfo, ILogger<MessageFactory> logger, IOptionsMonitor<EmailMessageOptions> optionsMonitor, TemplateEngine templateEngine)
    {
        this.cultureInfo = cultureInfo;
        this.logger = logger;
        this.optionsMonitor = optionsMonitor;
        this.templateEngine = templateEngine;
    }

    public async ValueTask<EmailMessage> CreateEmailAsync(Email email, CancellationToken cancellationToken) =>
        email switch
        {
            { CleaningSchedule: not null } =>
                await CreateMessageAsync(email.FromUrl, await CreateCleaningSchedule(email.CleaningSchedule, cancellationToken), cancellationToken),
            { ConfirmEmail: not null } => await CreateMessageAsync(email.FromUrl, email.ConfirmEmail, cancellationToken),
            { DebtReminder: not null } => await CreateMessageAsync(email.FromUrl, email.DebtReminder, cancellationToken),
            { LockBoxCodes: not null } => await CreateMessageAsync(email.FromUrl, email.LockBoxCodes, cancellationToken),
            { LockBoxCodesOverview: not null } => await CreateMessageAsync(email.FromUrl, email.LockBoxCodesOverview, cancellationToken),
            { NewOrder: not null } => await CreateMessageAsync(email.FromUrl, email.NewOrder, cancellationToken),
            { NewPassword: not null } => await CreateMessageAsync(email.FromUrl, email.NewPassword, cancellationToken),
            { NoFeeCancellationAllowed: not null } => await CreateMessageAsync(email.FromUrl, email.NoFeeCancellationAllowed, cancellationToken),
            { OrderConfirmed: not null } => await CreateMessageAsync(email.FromUrl, email.OrderConfirmed, cancellationToken),
            { OrderReceived: not null } => await CreateMessageAsync(email.FromUrl, email.OrderReceived, cancellationToken),
            { PayIn: not null } => await CreateMessageAsync(email.FromUrl, email.PayIn, cancellationToken),
            { PayOut: not null } => await CreateMessageAsync(email.FromUrl, email.PayOut, cancellationToken),
            { PostingsForMonth: not null } => await CreateMessageAsync(email.FromUrl, email.PostingsForMonth, cancellationToken),
            { ReservationsCancelled: not null } => await CreateMessageAsync(email.FromUrl, email.ReservationsCancelled, cancellationToken),
            { ReservationSettled: not null } => await CreateMessageAsync(email.FromUrl, email.ReservationSettled, cancellationToken),
            { SettlementNeeded: not null } => await CreateMessageAsync(email.FromUrl, email.SettlementNeeded, cancellationToken),
            { UserDeleted: not null } => await CreateMessageAsync(email.FromUrl, email.UserDeleted, cancellationToken),
            _ => throw new ArgumentException("Empty or unknown email.", nameof(email))
        };

    async ValueTask<EmailMessage> CreateMessageAsync<TModel>(Uri fromUrl, TModel model, CancellationToken cancellationToken) where TModel : MessageBase
    {
        try
        {
            var options = GetOptions();
            var emailModel = new EmailModel<TModel>(model, options.From!.Name!, fromUrl, cultureInfo);
            var subject = (await ExpandTemplateAsync(emailModel, TemplateType.Subject, cancellationToken)).TrimEnd('\r', '\n');
            var body = await ExpandTemplateAsync(emailModel, TemplateType.Body, cancellationToken);
            return new(options.From!.Email!, options.ReplyTo?.Email, new[] { model.Email.ToString() }, subject, body);
        }
        catch (Exception)
        {
            logger.LogWarning("Cannot create message {Model}", model);
            throw;
        }
    }

    EmailMessageOptions GetOptions() => ValidateOptions(optionsMonitor.CurrentValue);

    static EmailMessageOptions ValidateOptions(EmailMessageOptions options)
    {
        if (options.From?.Name is null || options.From.Email is null)
            throw new ConfigurationException("Missing or invalid 'from' email recipient.");
        if (options.ReplyTo is not null && (options.ReplyTo.Name is null || options.ReplyTo.Email is null))
            throw new ConfigurationException("Invalid 'reply to' email recipient.");
        return options;
    }

    async ValueTask<string> ExpandTemplateAsync<TModel>(EmailModel<TModel> model, TemplateType templateType, CancellationToken cancellationToken)
    {
        var compiledTemplate = await GetCompiledTemplateAsync<TModel>(templateType, cancellationToken);
        return await compiledTemplate.RunAsync(model);
    }

    async ValueTask<CompiledTemplate<EmailModel<TModel>>> GetCompiledTemplateAsync<TModel>(
        TemplateType templateType, CancellationToken cancellationToken)
    {
        var key = new TemplateKey(typeof(TModel), templateType);
        return await GetCompiledTemplateFromCache<TModel>(key, cancellationToken);
    }

    async ValueTask<CompiledTemplate<EmailModel<TModel>>> GetCompiledTemplateFromCache<TModel>(
        TemplateKey key, CancellationToken cancellationToken) =>
        templateCache.TryGetValue(key, out var compiledTemplate)
            ? (CompiledTemplate<EmailModel<TModel>>) compiledTemplate
            : await AddCompiledTemplateToCache<TModel>(key, cancellationToken);

    async ValueTask<CompiledTemplate<EmailModel<TModel>>> AddCompiledTemplateToCache<TModel>(TemplateKey key, CancellationToken cancellationToken)
    {
        var compiledTemplate = await CompileTemplateAsync<TModel>(key.TemplateType, cancellationToken);
        templateCache.Add(key, compiledTemplate);
        return compiledTemplate;
    }

    async ValueTask<CompiledTemplate<EmailModel<TModel>>> CompileTemplateAsync<TModel>(
        TemplateType templateType, CancellationToken cancellationToken)
    {
        var templateSource = await GetTemplateSourceAsync<TModel>(templateType, cancellationToken);
        return templateEngine.CompileTemplate<EmailModel<TModel>>(templateSource);
    }

    static async ValueTask<string> GetTemplateSourceAsync<TModel>(TemplateType templateType, CancellationToken cancellationToken)
    {
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetResourceName<TModel>(templateType))!;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().WaitAsync(cancellationToken);
    }

    static string GetResourceName<TModel>(TemplateType templateType) =>
        $"Frederikskaj2.Reservations.EmailSender.Messages.{typeof(TModel).Name}.{templateType}.cshtml";

    // ReSharper disable once NotAccessedPositionalProperty.Local
    record TemplateKey(Type ModelType, TemplateType TemplateType);

    enum TemplateType
    {
        Body,
        Subject
    }
}
