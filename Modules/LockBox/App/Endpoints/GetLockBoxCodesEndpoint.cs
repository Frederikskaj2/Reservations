using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.LockBox.GetLockBoxCodesShell;

namespace Frederikskaj2.Reservations.LockBox;

class GetLockBoxCodesEndpoint
{
    GetLockBoxCodesEndpoint() { }

    [Authorize(Roles = nameof(Roles.LockBoxCodes))]
    public static Task<IResult> Handle(
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetLockBoxCodesEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from codes in GetLockBoxCodes(entityReader, httpContext.RequestAborted)
            select new GetLockBoxCodesResponse(codes.Map(CreateWeeklyLockBoxCodes));
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }

    static WeeklyLockBoxCodesDto CreateWeeklyLockBoxCodes(WeeklyLockBoxCodes codes) =>
        new(
            codes.WeekNumber,
            codes.Date,
            codes.Codes.Map(CreateWeeklyLockBoxCode));

    static WeeklyLockBoxCodeDto CreateWeeklyLockBoxCode(WeeklyLockBoxCode code) =>
        new(
            code.ResourceId,
            code.Code,
            code.Difference);
}
