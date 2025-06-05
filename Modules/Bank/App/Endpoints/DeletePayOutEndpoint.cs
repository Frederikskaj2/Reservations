using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.DeletePayOutShell;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

class DeletePayOutEndpoint
{
    DeletePayOutEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromRoute] int payOutId,
        [FromHeader(Name = "If-Match")] string? eTag,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<DeletePayOutEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from validETag in Validator.ValidateETag(eTag).ToAsync()
            from _ in DeletePayOut(entityWriter, new(PayOutId.FromInt32(payOutId), validETag), httpContext.RequestAborted)
            select unit;
        return either.ToResult(logger, httpContext);
    }
}
