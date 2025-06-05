using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.LockBox.UpdateLockBoxCodesShell;

namespace Frederikskaj2.Reservations.LockBox;

class UpdateLockBoxCodesEndpoint
{
    UpdateLockBoxCodesEndpoint() { }

    [Authorize(Roles = nameof(Roles.LockBoxCodes))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] IEntityWriter entityWriter,
        [FromServices] ILogger<UpdateLockBoxCodesEndpoint> logger,
        HttpContext httpContext)
    {
        var either = UpdateLockBoxCodes(entityReader, entityWriter, new(dateProvider.Today), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
