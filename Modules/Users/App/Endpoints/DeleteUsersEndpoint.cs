using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.DeleteUsersShell;

namespace Frederikskaj2.Reservations.Users;

class DeleteUsersEndpoint
{
    DeleteUsersEndpoint() { }

    [Authorize(Roles = nameof(Roles.Jobs))]
    public static Task<IResult> Handle(
        IDateProvider dateProvider,
        IUsersEmailService emailService,
        IEntityReader entityReader,
        IEntityWriter entityWriter,
        ILogger<DeleteUsersEndpoint> logger,
        HttpContext httpContext)
    {
        var command = new DeleteUsersCommand(dateProvider.Now);
        var either = DeleteUsers(emailService, entityReader, entityWriter, command, httpContext.RequestAborted);
        return either.ToResult(logger, httpContext, includeFailureDetail: true);
    }
}
