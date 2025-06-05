using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.SendDebtRemindersShell;

namespace Frederikskaj2.Reservations.Bank;

class SendDebtRemindersEndpoint
{
    SendDebtRemindersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IBankEmailService emailService,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<SendDebtRemindersEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        HttpContext httpContext)
    {
        var either = SendDebtReminders(emailService, options.Value, entityReader, entityWriter, new(dateProvider.Now), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
