using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Client;

static class ClaimsPrincipalExtensions
{
    public static bool IsAuthenticated(this ClaimsPrincipal principal) => principal.Claims.Any();

    public static UserId UserId(this ClaimsPrincipal principal) =>
        Reservations.Shared.Core.UserId.FromInt32(int.Parse(principal.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value, CultureInfo.InvariantCulture));

    public static string? Email(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
}
