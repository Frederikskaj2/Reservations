﻿using System;
using System.Security.Claims;

namespace Frederikskaj2.Reservations.Shared
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? Id(this ClaimsPrincipal principal)
        {
            if (principal is null)
                throw new ArgumentNullException(nameof(principal));

            var nameIdentifierClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return nameIdentifierClaim != null && int.TryParse(nameIdentifierClaim.Value, out var id)
                ? (int?) id
                : null;
        }
    }
}