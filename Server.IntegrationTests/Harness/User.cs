using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public record User(string Email, string Password, string FullName, string Phone, ApartmentId ApartmentId, string AccountNumber);
