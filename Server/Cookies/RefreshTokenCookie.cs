using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Server;

public record RefreshTokenCookie(UserId? UserId, TokenId? TokenId);
