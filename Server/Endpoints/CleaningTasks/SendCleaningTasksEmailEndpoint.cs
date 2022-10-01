using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.Cleaning))]
public class SendCleaningTasksEmailEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<EmailAddress>
{
    readonly Func<LocalDate> getToday;
    readonly ILogger logger;
    readonly Func<SendCleaningTasksEmailCommand, EitherAsync<Failure, EmailAddress>> sendCleaningTasks;

    public SendCleaningTasksEmailEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<SendCleaningTasksEmailEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        getToday = () => dateProvider.Today;
        sendCleaningTasks = command => SendCleaningTasksEmailHandler.Handle(contextFactory, emailService, options.Value, command);
    }

    [HttpPost("cleaning-tasks/send")]
    public override Task<ActionResult<EmailAddress>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new SendCleaningTasksEmailCommand(userId, getToday())
            from email in sendCleaningTasks(command)
            select email;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
