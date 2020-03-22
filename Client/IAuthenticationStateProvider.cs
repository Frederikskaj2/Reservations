using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Client
{
    public interface IAuthenticationStateProvider
    {
        void UpdateUser(AuthenticatedUser user);
    }
}