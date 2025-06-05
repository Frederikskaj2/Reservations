using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Globalization;
using System.Security.Claims;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Core;

public static class ClaimsPrincipalExtensions
{
    public static Option<UserId> UserId(this ClaimsPrincipal principal)
    {
        var nameIdentifierClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return nameIdentifierClaim is not null && int.TryParse(nameIdentifierClaim.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? Some(Users.UserId.FromInt32(id))
            : None;
    }
}
