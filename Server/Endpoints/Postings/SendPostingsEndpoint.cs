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

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public class SendPostingsEndpoint : EndpointBaseAsync.WithRequest<MonthServerRequest>.WithActionResult<EmailAddress>
{
    readonly ILogger logger;
    readonly Func<SendPostingsCommand, EitherAsync<Failure, EmailAddress>> sendPostings;
    readonly Func<string?, EitherAsync<Failure, LocalDate>> validateMonth;

    public SendPostingsEndpoint(IPersistenceContextFactory contextFactory, IEmailService emailService, ILogger<SendPostingsEndpoint> logger)
    {
        this.logger = logger;
        validateMonth = month => Validator.ValidateMonth(month).ToAsync();
        sendPostings = command => SendPostingsHandler.Handle(contextFactory, emailService, command);
    }

    [HttpPost("postings/send")]
    public override Task<ActionResult<EmailAddress>> HandleAsync([FromQuery] MonthServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from date in validateMonth(request.Month)
            let command = new SendPostingsCommand(userId, date)
            from email in sendPostings(command)
            select email;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
