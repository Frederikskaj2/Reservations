using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Security.Claims;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

static class ClaimsPrincipalExtensions
{
    public static Option<UserId> Id(this ClaimsPrincipal principal)
    {
        var nameIdentifierClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return nameIdentifierClaim is not null && int.TryParse(nameIdentifierClaim.Value, out var id)
            ? Some(UserId.FromInt32(id))
            : None;
    }
}
