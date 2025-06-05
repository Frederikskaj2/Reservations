using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Bank.GetPostingsShell;
using static Frederikskaj2.Reservations.Bank.PostingsFactory;

namespace Frederikskaj2.Reservations.Bank;

class GetPostingsEndpoint
{
    GetPostingsEndpoint() { }

    [Authorize(Roles = nameof(Roles.Bookkeeping))]
    public static Task<IResult> Handle(
        [FromQuery] string? month,
        [FromServices] IEntityReader entityReader,
        [FromServices] ILogger<GetPostingsEndpoint> logger,
        HttpContext httpContext)
    {
        var either =
            from validMonth in Validator.ValidateMonth(month).ToAsync()
            let query = new GetPostingsQuery(validMonth)
            from postingsForMonth in GetPostings(entityReader, query, httpContext.RequestAborted)
            select new GetPostingsResponse(CreatePostings(postingsForMonth));
        return either.ToResult(logger, httpContext);
    }
}
