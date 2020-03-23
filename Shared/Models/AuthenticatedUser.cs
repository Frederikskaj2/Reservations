namespace Frederikskaj2.Reservations.Shared
{
    public class AuthenticatedUser
    {
        public static readonly AuthenticatedUser UnknownUser = new AuthenticatedUser();

        public int? Id { get; set; }
        public string? Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAdministrator { get; set; }
        public int? ApartmentId { get; set; }
    }
}