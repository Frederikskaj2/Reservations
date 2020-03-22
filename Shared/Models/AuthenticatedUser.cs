namespace Frederikskaj2.Reservations.Shared
{
    public class AuthenticatedUser
    {
        private AuthenticatedUser()
        {
        }

        public AuthenticatedUser(int? id, string? name, bool isAuthenticated, bool isAdministrator, int? apartmentId)
        {
            Id = id;
            Name = name;
            IsAuthenticated = isAuthenticated;
            IsAdministrator = isAdministrator;
            ApartmentId = apartmentId;
        }

        public int? Id { get; }
        public string? Name { get; }
        public bool IsAuthenticated { get; }
        public bool IsAdministrator { get; }
        public int? ApartmentId { get; }

        public static readonly AuthenticatedUser UnknownUser = new AuthenticatedUser();
    }
}