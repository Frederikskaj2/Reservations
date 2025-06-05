using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.CreatePayOutShell;
using static Frederikskaj2.Reservations.Bank.PayOutFactory;
using static Frederikskaj2.Reservations.Bank.Validator;

namespace Frederikskaj2.Reservations.Bank;

class CreatePayOutEndpoint
{
    CreatePayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromBody] CreatePayOutRequest request,
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<CreatePayOutEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from command in ValidateCreatePayOut(request, dateProvider.Now).ToAsync()
            from payOutDetails in CreatePayOut(entityReader, entityWriter, command, httpContext.RequestAborted)
            select WithETag.Create(new CreatePayOutResponse(CreatePayOut(payOutDetails)), payOutDetails.ETag.ToString());
        return either.ToResult(logger, httpContext);
    }
}
