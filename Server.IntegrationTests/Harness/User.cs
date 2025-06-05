using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public record User(string Email, string Password, string FullName, string Phone, ApartmentId ApartmentId, string AccountNumber);
