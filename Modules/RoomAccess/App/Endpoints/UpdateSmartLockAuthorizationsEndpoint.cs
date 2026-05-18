using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.RoomAccess.UpdateSmartLockAuthorizationsShell;

namespace Frederikskaj2.Reservations.RoomAccess;

class UpdateSmartLockAuthorizationsEndpoint
{
    UpdateSmartLockAuthorizationsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        [FromServices] IDateProvider dateProvider,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<UpdateSmartLockAuthorizationsEndpoint> logger,
        [FromServices] IOptionsSnapshot<OrderingOptions> options,
        [FromServices] ISmartLockService smartLockService,
        [FromServices] ITimeConverter timeConverter,
        HttpContext httpContext)
    {
        var either = UpdateSmartLockAuthorizations(
            options.Value, entityReader, smartLockService, timeConverter, new(dateProvider.Today), httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
