namespace Frederikskaj2.Reservations.Users;

public interface IAuthenticationService
{
    Tokens CreateTokens(AuthenticatedUser authenticatedUser);
}
