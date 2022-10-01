using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.LockBoxCodes))]
public class SendLockBoxCodesEndpoint : EndpointBaseAsync.WithoutRequest.WithActionResult<EmailAddress>
{
    readonly Func<LocalDate> getToday;
    readonly ILogger logger;
    readonly Func<SendWeeklyLockBoxCodesCommand, EitherAsync<Failure, EmailAddress>> sendWeeklyLockBoxCodes;

    public SendLockBoxCodesEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<SendLockBoxCodesEndpoint> logger)
    {
        this.logger = logger;
        getToday = () => dateProvider.Today;
        sendWeeklyLockBoxCodes = command => SendWeeklyLockBoxCodesHandler.Handle(contextFactory, emailService, command);
    }

    [HttpPost("lock-box-codes/send")]
    public override Task<ActionResult<EmailAddress>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            let command = new SendWeeklyLockBoxCodesCommand(userId, getToday())
            from email in sendWeeklyLockBoxCodes(command)
            select email;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
