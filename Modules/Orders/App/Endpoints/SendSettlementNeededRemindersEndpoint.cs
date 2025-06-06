﻿using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.SendSettlementNeededRemindersShell;

namespace Frederikskaj2.Reservations.Orders;

class SendSettlementNeededRemindersEndpoint
{
    SendSettlementNeededRemindersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IOrdersEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SendSettlementNeededRemindersEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        HttpContext httpContext)
    {
        var either = SendSettlementNeededReminders(emailService, options.Value, entityReader, entityWriter, new(dateProvider.Today), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
