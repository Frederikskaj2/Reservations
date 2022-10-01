using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
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
    readonly IOptionsMonitor<EmailMessageOptions> optionsMonitor;
    readonly TemplateEngine templateEngine;

    public MessageFactory(CultureInfo cultureInfo, IOptionsMonitor<EmailMessageOptions> optionsMonitor, TemplateEngine templateEngine)
    {
        this.cultureInfo = cultureInfo;
        this.optionsMonitor = optionsMonitor;
        this.templateEngine = templateEngine;
    }

    public async ValueTask<EmailMessage> CreateEmailAsync(Email email, CancellationToken cancellationToken) =>
        email switch
        {
            { CleaningSchedule: { } } =>
                await CreateMessageAsync(email.FromUrl, await CreateCleaningSchedule(email.CleaningSchedule, cancellationToken), cancellationToken),
            { ConfirmEmail: { } } => await CreateMessageAsync(email.FromUrl, email.ConfirmEmail, cancellationToken),
            { DebtReminder: { } } => await CreateMessageAsync(email.FromUrl, email.DebtReminder, cancellationToken),
            { LockBoxCodes: {} } => await CreateMessageAsync(email.FromUrl, email.LockBoxCodes, cancellationToken),
            { LockBoxCodesOverview: { } } => await CreateMessageAsync(email.FromUrl, email.LockBoxCodesOverview, cancellationToken),
            { NewOrder: { } } => await CreateMessageAsync(email.FromUrl, email.NewOrder, cancellationToken),
            { NewPassword: { } } => await CreateMessageAsync(email.FromUrl, email.NewPassword, cancellationToken),
            { NoFeeCancellationAllowed: { } } => await CreateMessageAsync(email.FromUrl, email.NoFeeCancellationAllowed, cancellationToken),
            { OrderConfirmed: { } } => await CreateMessageAsync(email.FromUrl, email.OrderConfirmed, cancellationToken),
            { OrderReceived: { } } => await CreateMessageAsync(email.FromUrl, email.OrderReceived, cancellationToken),
            { PayIn: { } } => await CreateMessageAsync(email.FromUrl, email.PayIn, cancellationToken),
            { PayOut: { } } => await CreateMessageAsync(email.FromUrl, email.PayOut, cancellationToken),
            { PostingsForMonth: { } } => await CreateMessageAsync(email.FromUrl, email.PostingsForMonth, cancellationToken),
            { ReservationsCancelled: { } } => await CreateMessageAsync(email.FromUrl, email.ReservationsCancelled, cancellationToken),
            { ReservationSettled: { } } => await CreateMessageAsync(email.FromUrl, email.ReservationSettled, cancellationToken),
            { SettlementNeeded: { } } => await CreateMessageAsync(email.FromUrl, email.SettlementNeeded, cancellationToken),
            { UserDeleted: { } } => await CreateMessageAsync(email.FromUrl, email.UserDeleted, cancellationToken),
            _ => throw new ArgumentException("Empty or unknown email.", nameof(email))
        };

    async ValueTask<EmailMessage> CreateMessageAsync<TModel>(Uri fromUrl, TModel model, CancellationToken cancellationToken) where TModel : MessageBase
    {
        var options = GetOptions();
        var emailModel = new EmailModel<TModel>(model, options.From!.Name!, fromUrl, cultureInfo);
        var subject = (await ExpandTemplateAsync(emailModel, TemplateType.Subject, cancellationToken)).TrimEnd('\r', '\n');
        var body = await ExpandTemplateAsync(emailModel, TemplateType.Body, cancellationToken);
        return new(options.From!.Email!, options.ReplyTo?.Email, new[] { model.Email.ToString() }, subject, body);
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

    record TemplateKey(Type ModelType, TemplateType TemplateType);

    enum TemplateType
    {
        Body,
        Subject
    }
}
